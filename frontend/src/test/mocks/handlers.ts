import { http, HttpResponse } from 'msw';
import { FleetDto } from '../../hooks/useFleets';

const API_BASE = 'http://localhost:5000/api/v1';

export const handlers = [
  // GET /fleets
  http.get(`${API_BASE}/fleets`, ({ request }) => {
    const url = new URL(request.url);
    const pageNumber = Number(url.searchParams.get('pageNumber')) || 1;
    const pageSize = Number(url.searchParams.get('pageSize')) || 20;

    const mockFleet: FleetDto = {
      id: 'mock-fleet-id-1',
      name: 'Mock Fleet Alpha',
      homePortId: 'port-1',
      homePortName: 'Mock Port',
      status: 'Active',
      description: 'A mock fleet for testing'
    };

    return HttpResponse.json({
      items: [mockFleet],
      totalCount: 1,
      pageNumber,
      pageSize,
      totalPages: 1
    });
  }),

  // POST /fleets
  http.post(`${API_BASE}/fleets`, async ({ request }) => {
    const body = await request.json() as any;
    
    // Simulate duplicate name conflict
    if (body.name === 'Conflict Fleet') {
      return HttpResponse.json(
        { message: 'A fleet with this name already exists.' },
        { status: 409 }
      );
    }

    const newFleet: FleetDto = {
      id: 'mock-new-fleet-id',
      name: body.name,
      homePortId: body.homePortId,
      homePortName: 'Unknown Port',
      status: body.status || 'Active',
      description: body.description
    };

    return HttpResponse.json(newFleet, { status: 201 });
  }),

  // PUT /fleets/:id
  http.put(`${API_BASE}/fleets/:id`, async ({ params, request }) => {
    const { id } = params;
    const body = await request.json() as any;
    
    return HttpResponse.json({
      id: id,
      name: body.name,
      homePortId: body.homePortId,
      homePortName: 'Unknown Port',
      status: body.status,
      description: body.description
    });
  }),

  // GET /ports (used by PortSelect in FleetFormModal)
  http.get(`${API_BASE}/ports`, () => {
    return HttpResponse.json({
      items: [
        { id: 'port-1', name: 'Mock Port', unLocode: 'MOCK' }
      ]
    });
  })
];
