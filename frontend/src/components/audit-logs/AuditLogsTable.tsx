import React, { useState } from 'react';
import { AuditLog } from '../../types/auditLog';
import { Badge } from '../ui/Badge';
import { AuditLogChangesModal } from './AuditLogChangesModal';
import { Table } from '../ui/Table';

interface AuditLogsTableProps {
  logs: AuditLog[];
}

const getActionColor = (action: string) => {
  switch (action) {
    case 'Created': return 'green';
    case 'Updated': return 'blue';
    case 'Deleted': return 'red';
    default: return 'gray';
  }
};

const formatGuid = (guid: string) => {
  if (!guid) return '-';
  if (guid.length > 8) {
    return `${guid.substring(0, 8)}...`;
  }
  return guid;
};

export const AuditLogsTable: React.FC<AuditLogsTableProps> = ({ logs }) => {
  const [selectedChanges, setSelectedChanges] = useState<string | null>(null);

  const columns = [
    {
      key: 'timestamp',
      header: 'Timestamp',
      accessor: (log: AuditLog) => new Date(log.timestamp).toLocaleString(),
    },
    {
      key: 'userName',
      header: 'User Name',
      accessor: (log: AuditLog) => log.userName,
    },
    {
      key: 'action',
      header: 'Action',
      accessor: (log: AuditLog) => <Badge text={log.action} color={getActionColor(log.action) as any} />,
    },
    {
      key: 'entityName',
      header: 'Entity Name',
      accessor: (log: AuditLog) => log.entityName,
    },
    {
      key: 'entityId',
      header: 'Entity ID',
      accessor: (log: AuditLog) => (
        <span title={log.entityId} className="font-mono text-xs text-text-muted">
          {formatGuid(log.entityId)}
        </span>
      ),
    },
    {
      key: 'actions',
      header: 'Actions',
      accessor: (log: AuditLog) => (
        <button
          onClick={() => setSelectedChanges(log.changes)}
          disabled={!log.changes || log.action !== 'Updated'}
          className={`text-sm font-medium ${!log.changes || log.action !== 'Updated' ? 'text-text-muted cursor-not-allowed' : 'text-indigo-400 hover:text-indigo-300 transition-colors'}`}
        >
          View Changes
        </button>
      ),
    },
  ];

  return (
    <>
      <Table data={logs} columns={columns} />
      {selectedChanges && (
        <AuditLogChangesModal
          changes={selectedChanges}
          onClose={() => setSelectedChanges(null)}
        />
      )}
    </>
  );
};
