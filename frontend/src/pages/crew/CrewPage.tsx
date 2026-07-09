import React, { useState } from 'react';
import { useCrewQuery, useDeactivateCrewMemberMutation, useUnassignFromShipMutation } from '../../hooks/useCrew';
import { useAuthStore } from '../../store/authStore';
import { CrewTable } from '../../components/crew/CrewTable';
import { CrewFormModal } from '../../components/crew/CrewFormModal';
import { AssignToShipModal } from '../../components/crew/AssignToShipModal';
import { Pagination } from '../../components/ui/Pagination';
import { Button } from '../../components/ui/Button';
import { ShipSelect } from '../../components/ships/ShipSelect';
import { CREW_STATUSES, CREW_RANKS } from '../../lib/constants';
import { CrewMember } from '../../types/crew';
import { ExportButton } from '../../components/ui/ExportButton';

export const CrewPage: React.FC = () => {
  const user = useAuthStore(state => state.user);
  const canWrite = user?.roles.includes('Admin') || user?.roles.includes('CrewManager') || false;

  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [shipId, setShipId] = useState('');
  const [status, setStatus] = useState('');
  const [rank, setRank] = useState('');
  const [unassigned, setUnassigned] = useState(false);

  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [editingCrew, setEditingCrew] = useState<CrewMember | undefined>();
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [assigningCrew, setAssigningCrew] = useState<CrewMember | undefined>();

  const { data, isLoading } = useCrewQuery({
    pageNumber: page,
    pageSize: 10,
    searchTerm,
    shipId: unassigned ? undefined : shipId,
    status,
    rank,
    unassigned: unassigned ? true : undefined,
  });

  const { mutateAsync: deactivateCrew } = useDeactivateCrewMemberMutation();
  const { mutateAsync: unassignFromShip } = useUnassignFromShipMutation();

  const handleEdit = (crew: CrewMember) => {
    setEditingCrew(crew);
    setIsFormModalOpen(true);
  };

  const handleCreate = () => {
    setEditingCrew(undefined);
    setIsFormModalOpen(true);
  };

  const handleAssign = (crew: CrewMember) => {
    setAssigningCrew(crew);
    setIsAssignModalOpen(true);
  };

  const handleUnassign = async (crew: CrewMember) => {
    if (window.confirm(`Are you sure you want to unassign ${crew.firstName} ${crew.lastName} from their ship?`)) {
      await unassignFromShip(crew.id);
    }
  };

  const handleDeactivate = async (crew: CrewMember) => {
    if (window.confirm(`Are you sure you want to deactivate ${crew.firstName} ${crew.lastName}?`)) {
      await deactivateCrew(crew.id);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-text-primary">Crew Management</h1>
        <div className="flex items-center gap-2">
          <ExportButton 
            exportPath="/crew/export" 
            filters={{
              searchTerm,
              shipId: unassigned ? undefined : shipId,
              status,
              rank,
              unassigned: unassigned ? true : undefined,
            }} 
          />
          {canWrite && (
            <Button variant="primary" onClick={handleCreate}>
              Add Crew Member
            </Button>
          )}
        </div>
      </div>

      <div className="bg-surface p-4 rounded-lg flex flex-wrap gap-4 items-center">
        <div className="flex-1 min-w-[200px]">
          <input
            type="text"
            placeholder="Search by name or license..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
          />
        </div>
        
        <div className="w-48">
          <select
            value={rank}
            onChange={(e) => setRank(e.target.value)}
            className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
          >
            <option value="">All Ranks</option>
            {CREW_RANKS.map(r => <option key={r} value={r}>{r}</option>)}
          </select>
        </div>

        <div className="w-48">
          <select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
          >
            <option value="">All Statuses</option>
            {CREW_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
          </select>
        </div>

        <div className="w-48">
          <ShipSelect 
            value={shipId} 
            onChange={setShipId} 
            placeholder="Filter by Ship"
            disabled={unassigned}
          />
        </div>

        <div className="flex items-center gap-2 text-text-primary">
          <input
            type="checkbox"
            id="unassigned"
            checked={unassigned}
            onChange={(e) => {
              setUnassigned(e.target.checked);
              if (e.target.checked) setShipId('');
            }}
            className="rounded border-border text-primary-600 focus:ring-primary-500 bg-background"
          />
          <label htmlFor="unassigned" className="cursor-pointer">Unassigned only</label>
        </div>
      </div>

      <CrewTable
        crew={data?.items || []}
        isLoading={isLoading}
        canWrite={canWrite}
        onEdit={handleEdit}
        onDeactivate={handleDeactivate}
        onAssign={handleAssign}
        onUnassign={handleUnassign}
      />

      {data && (
        <Pagination
          pageNumber={page}
          totalPages={Math.ceil(data.totalCount / 10)}
          onPageChange={setPage}
        />
      )}

      {isFormModalOpen && (
        <CrewFormModal
          isOpen={isFormModalOpen}
          onClose={() => setIsFormModalOpen(false)}
          crewMember={editingCrew}
        />
      )}

      {isAssignModalOpen && assigningCrew && (
        <AssignToShipModal
          isOpen={isAssignModalOpen}
          onClose={() => setIsAssignModalOpen(false)}
          crewMemberId={assigningCrew.id}
        />
      )}
    </div>
  );
};
