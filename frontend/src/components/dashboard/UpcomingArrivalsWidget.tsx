import React from 'react';
import { useVoyagesQuery } from '../../hooks/useVoyages';
import { Badge } from '../ui/Badge';
import { format } from 'date-fns';

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Scheduled': return 'blue';
    case 'Delayed': return 'amber';
    case 'InTransit': return 'purple';
    default: return 'gray';
  }
};

export const UpcomingArrivalsWidget: React.FC = () => {
  const { data, isLoading } = useVoyagesQuery({
    pageNumber: 1,
    pageSize: 100,
    sortBy: 'departureDate'
  });

  if (isLoading) {
    return (
      <div className="bg-surface rounded-lg p-6 border border-border h-80 flex flex-col items-center justify-center">
        <div className="text-text-muted">Loading upcoming arrivals...</div>
      </div>
    );
  }

  const now = new Date();
  const nextWeek = new Date();
  nextWeek.setDate(now.getDate() + 7);

  const upcomingVoyages = (data?.items || [])
    .filter(v => {
      if (v.status === 'Completed' || v.status === 'Cancelled') return false;
      const arrival = new Date(v.estimatedArrivalDate);
      return arrival >= now && arrival <= nextWeek;
    })
    .sort((a, b) => new Date(a.estimatedArrivalDate).getTime() - new Date(b.estimatedArrivalDate).getTime())
    .slice(0, 10);

  return (
    <div className="bg-surface rounded-lg shadow-sm border border-border overflow-hidden flex flex-col h-80">
      <div className="px-6 py-5 border-b border-border flex justify-between items-center bg-surface shrink-0">
        <h3 className="text-lg font-semibold leading-7 text-text-primary">Upcoming Arrivals (Next 7 Days)</h3>
      </div>
      <div className="flex-1 overflow-y-auto">
        {upcomingVoyages.length === 0 ? (
          <div className="flex h-full items-center justify-center p-6 text-text-muted">
            No voyages arriving in the next 7 days.
          </div>
        ) : (
          <ul className="divide-y divide-border">
            {upcomingVoyages.map(voyage => (
              <li key={voyage.id} className="p-4 sm:px-6 hover:bg-slate-750 transition-colors">
                <div className="flex items-center justify-between">
                  <div className="flex flex-col gap-1">
                    <p className="text-sm font-medium text-text-primary truncate">
                      {voyage.voyageNumber} - {voyage.shipName}
                    </p>
                    <p className="mt-1 flex items-center text-sm text-text-muted">
                      Arriving at {voyage.destinationPortName}
                    </p>
                  </div>
                  <div className="flex flex-col items-end gap-1">
                    <p className="text-sm text-text-primary">
                      {format(new Date(voyage.estimatedArrivalDate), 'MMM d, yyyy')}
                    </p>
                    <Badge text={voyage.status} color={getStatusColor(voyage.status) as any} />
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
};
