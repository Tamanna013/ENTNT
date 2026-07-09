import React from 'react';
import { useIncidentsQuery } from '../../hooks/useIncidents';
import { Badge } from '../ui/Badge';
import { AlertTriangle, Clock } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Incident } from '../../types/incident';

const severityOrder: Record<string, number> = {
  'Critical': 1,
  'High': 2,
  'Medium': 3,
  'Low': 4
};

export const OpenIncidentsWidget: React.FC = () => {
  const { data, isLoading, error } = useIncidentsQuery({ pageNumber: 1, pageSize: 50 });

  if (isLoading) {
    return (
      <div className="bg-surface rounded-lg shadow-sm border border-border p-6 flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="bg-surface rounded-lg shadow-sm border border-border p-6 flex flex-col items-center justify-center text-red-400 h-64">
        <AlertTriangle className="h-8 w-8 mb-2" />
        <p>Failed to load incidents</p>
      </div>
    );
  }

  const openIncidents = data.items
    .filter(i => i.status === 'Reported' || i.status === 'UnderInvestigation')
    .sort((a, b) => {
      const orderA = severityOrder[a.severity] || 99;
      const orderB = severityOrder[b.severity] || 99;
      if (orderA !== orderB) return orderA - orderB;
      return new Date(b.occurredAt).getTime() - new Date(a.occurredAt).getTime();
    })
    .slice(0, 10);

  return (
    <div className="bg-surface rounded-lg shadow-sm border border-border flex flex-col h-full">
      <div className="p-4 border-b border-border flex justify-between items-center bg-slate-800/50">
        <div className="flex items-center gap-2">
          <AlertTriangle className="h-5 w-5 text-indigo-400" />
          <h2 className="text-lg font-medium text-text-primary">Open Incidents</h2>
        </div>
        <Link to="/incidents" className="text-sm text-indigo-400 hover:text-indigo-300">
          View all
        </Link>
      </div>

      <div className="flex-1 p-4 overflow-y-auto max-h-[400px]">
        {openIncidents.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-text-muted gap-2 py-8">
            <Clock className="w-8 h-8 opacity-20" />
            <p>No open incidents.</p>
          </div>
        ) : (
          <ul className="space-y-3">
            {openIncidents.map((incident: Incident) => (
              <li key={incident.id} className="bg-slate-900/50 p-3 rounded border border-slate-700/50 flex flex-col gap-2">
                <div className="flex justify-between items-start gap-2">
                  <div>
                    <h3 className="text-sm font-medium text-text-primary line-clamp-1">{incident.title}</h3>
                    <p className="text-xs text-text-muted">{incident.shipName}</p>
                  </div>
                  <div className="flex flex-col items-end gap-1 shrink-0">
                    <Badge color={incident.status === 'Reported' ? 'yellow' : 'blue'} text={incident.status} />
                  </div>
                </div>
                <div className="flex justify-between items-center">
                  <div className="flex items-center gap-1.5">
                    <Badge 
                      color={incident.severity === 'Critical' ? 'red' : incident.severity === 'High' ? 'yellow' : 'gray'} 
                      text={incident.severity} 
                    />
                    {incident.severity === 'Critical' && (
                      <AlertTriangle className="h-3.5 w-3.5 text-red-500 animate-pulse" />
                    )}
                  </div>
                  <span className="text-xs text-text-muted">
                    {new Date(incident.occurredAt).toLocaleDateString()}
                  </span>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
};
