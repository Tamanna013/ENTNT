import React, { useMemo } from 'react';
import { Table, Column } from '../ui/Table';
import { AiUsageReportRow } from '../../types/aiUsage';

interface AiUsageTableProps {
  data: AiUsageReportRow[];
}

export const AiUsageTable: React.FC<AiUsageTableProps> = ({ data }) => {
  const columns = useMemo<Column<AiUsageReportRow>[]>(() => [
    { key: 'userName', header: 'User Name', sortable: true },
    { key: 'requestCount', header: 'Request Count', sortable: true },
    { 
      key: 'successCount', 
      header: 'Success Count', 
      render: (row) => (
        <>
          {row.successCount} / {row.requestCount}
          {row.requestCount > 0 && (
            <span className="text-text-muted text-sm ml-2">
              ({Math.round((row.successCount / row.requestCount) * 100)}%)
            </span>
          )}
        </>
      )
    },
    { 
      key: 'totalTokensUsed', 
      header: 'Total Tokens Used',
      render: (row) => <>{row.totalTokensUsed ?? '—'}</>
    }
  ], []);

  return (
    <div className="bg-surface rounded-lg border border-border overflow-hidden">
      <Table 
        columns={columns} 
        data={data} 
        emptyMessage="No usage data found." 
      />
    </div>
  );
};
