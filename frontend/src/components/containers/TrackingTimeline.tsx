import React from 'react';
import { ContainerTrackingEvent } from '../../types/container';
import { MapPin } from 'lucide-react';

interface TrackingTimelineProps {
  events: ContainerTrackingEvent[];
}

export const TrackingTimeline: React.FC<TrackingTimelineProps> = ({ events }) => {
  if (!events || events.length === 0) {
    return <p className="text-sm text-text-muted">No tracking history available.</p>;
  }

  return (
    <div className="flow-root">
      <ul className="-mb-8">
        {events.map((event, eventIdx) => (
          <li key={event.id}>
            <div className="relative pb-8">
              {eventIdx !== events.length - 1 ? (
                <span className="absolute left-4 top-4 -ml-px h-full w-0.5 bg-gray-700" aria-hidden="true" />
              ) : null}
              <div className="relative flex space-x-3">
                <div>
                  <span className="h-8 w-8 rounded-full bg-indigo-500/10 flex items-center justify-center ring-8 ring-slate-900">
                    <MapPin className="h-4 w-4 text-indigo-400" aria-hidden="true" />
                  </span>
                </div>
                <div className="flex min-w-0 flex-1 justify-between space-x-4 pt-1.5">
                  <div>
                    <p className="text-sm font-semibold text-text-primary">{event.eventType} <span className="font-normal text-text-muted">at {event.location}</span></p>
                    {event.notes && <p className="mt-1 text-sm text-text-muted">{event.notes}</p>}
                    <p className="mt-1 text-xs text-gray-500">Recorded by {event.recordedByUserName}</p>
                  </div>
                  <div className="whitespace-nowrap text-right text-sm text-text-muted">
                    <time dateTime={event.timestamp}>
                      {new Date(event.timestamp).toLocaleString(undefined, { 
                        month: 'short', day: 'numeric', year: 'numeric', 
                        hour: 'numeric', minute: '2-digit' 
                      })}
                    </time>
                  </div>
                </div>
              </div>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};
