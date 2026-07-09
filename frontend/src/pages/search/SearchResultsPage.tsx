import React, { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useNaturalLanguageSearch } from '../../hooks/useNaturalLanguageSearch';
import { InterpretedQueryBanner } from '../../components/search/InterpretedQueryBanner';
import { AiUnavailableBanner } from '../../components/assistant/AiUnavailableBanner';
import { CargoTable } from '../../components/cargo/CargoTable';
import { VoyagesTable } from '../../components/voyages/VoyagesTable';
import { ShipsTable } from '../../components/ships/ShipsTable';
import { IncidentsTable } from '../../components/incidents/IncidentsTable';
import { Cargo } from '../../types/cargo';
import { Voyage } from '../../types/voyage';
import { Ship } from '../../types/ship';
import { Incident } from '../../types/incident';
import { PagedResult } from '../../types/pagination';

export const SearchResultsPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const q = searchParams.get('q') || '';
  
  const { mutate: search, data, isPending, isError } = useNaturalLanguageSearch();

  useEffect(() => {
    if (q) {
      search(q);
    }
  }, [q, search]);

  const renderResults = () => {
    if (!data || !data.results) return null;

    switch (data.interpretedModule) {
      case 'Cargo':
        return <CargoTable data={(data.results as PagedResult<Cargo>).items} isLoading={false} canWrite={false} />;
      case 'Voyage':
        return <VoyagesTable voyages={(data.results as PagedResult<Voyage>).items} canWrite={false} onEdit={() => {}} onDelete={() => {}} />;
      case 'Ship':
        return <ShipsTable ships={(data.results as PagedResult<Ship>).items} canWrite={false} onEdit={() => {}} onDeactivate={() => {}} />;
      case 'Incident':
        return <IncidentsTable incidents={(data.results as PagedResult<Incident>).items} isLoading={false} />;
      default:
        return null;
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">
          Search Results
        </h1>
      </div>

      {isPending && (
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
        </div>
      )}

      {isError && (
        <div className="bg-red-50 text-red-600 p-4 rounded-lg">
          An error occurred while searching. Please try again.
        </div>
      )}

      {data && (
        <>
          {!data.isAvailable ? (
            <AiUnavailableBanner />
          ) : !data.interpretedModule ? (
            <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 p-6 rounded-lg text-center">
              <h3 className="text-lg font-medium mb-2">{data.message || "I couldn't understand that query."}</h3>
              <p>Try rephrasing your search to be more specific.</p>
            </div>
          ) : (
            <>
              <InterpretedQueryBanner 
                module={data.interpretedModule} 
                filters={data.interpretedFilters} 
              />
              {renderResults()}
            </>
          )}
        </>
      )}
    </div>
  );
};
