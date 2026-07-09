import React from 'react';
import { useAuthStore } from '../store/authStore';
import { useFleetsQuery } from '../hooks/useFleets';
import { useShipsQuery } from '../hooks/useShips';
import { useCrewQuery } from '../hooks/useCrew';
import { Anchor, Ship, Users } from 'lucide-react';
import { SummaryCard } from '../components/dashboard/SummaryCard';
import { ShipsPerFleetChart } from '../components/dashboard/ShipsPerFleetChart';
import { CrewStatusChart } from '../components/dashboard/CrewStatusChart';
import { ExpiringCertificationsWidget } from '../components/dashboard/ExpiringCertificationsWidget';
import { UpcomingArrivalsWidget } from '../components/dashboard/UpcomingArrivalsWidget';
import { CargoByTypeChart } from '../components/dashboard/CargoByTypeChart';
import { MapPin } from 'lucide-react';
import { useVoyagesQuery } from '../hooks/useVoyages';
import { FuelEfficiencyWidget } from '../components/dashboard/FuelEfficiencyWidget';
import { UpcomingMaintenanceWidget } from '../components/dashboard/UpcomingMaintenanceWidget';
import { RecentNotificationsWidget } from '../components/dashboard/RecentNotificationsWidget';
import { DashboardSection } from '../components/dashboard/DashboardSection';
import { OpenIncidentsWidget } from '../components/dashboard/OpenIncidentsWidget';

export const DashboardPage: React.FC = () => {
    const user = useAuthStore(state => state.user);

    // Fetch counts using pageNumber 1 and pageSize 1 to minimize payload size
    const { data: fleetsData } = useFleetsQuery({ pageNumber: 1, pageSize: 1 });
    const { data: shipsData } = useShipsQuery({ pageNumber: 1, pageSize: 1 });
    const { data: crewData } = useCrewQuery({ pageNumber: 1, pageSize: 1 });
    const { data: voyagesData } = useVoyagesQuery({ pageNumber: 1, pageSize: 1, status: 'InTransit' });

    return (
        <div className="space-y-6">
            <h1 className="text-2xl font-bold text-text-primary mb-6">
                Welcome back, {user?.firstName}
            </h1>
            
            <DashboardSection title="Fleet Overview">
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-6">
                    <SummaryCard 
                        icon={Anchor} 
                        label="Total Fleets" 
                        value={fleetsData?.totalCount ?? '—'} 
                    />
                    <SummaryCard 
                        icon={Ship} 
                        label="Total Ships" 
                        value={shipsData?.totalCount ?? '—'} 
                    />
                    <SummaryCard 
                        icon={Users} 
                        label="Total Crew" 
                        value={crewData?.totalCount ?? '—'} 
                    />
                    <SummaryCard 
                        icon={MapPin} 
                        label="Active Voyages" 
                        value={voyagesData?.totalCount ?? '—'} 
                    />
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <ShipsPerFleetChart />
                    <CrewStatusChart />
                </div>
            </DashboardSection>

            <DashboardSection title="Operations">
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    <div className="lg:col-span-2">
                        <UpcomingArrivalsWidget />
                    </div>
                    <div>
                        <CargoByTypeChart />
                    </div>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <UpcomingMaintenanceWidget />
                    <FuelEfficiencyWidget />
                </div>
            </DashboardSection>

            <DashboardSection title="Alerts & Notifications">
                <div className="w-full mb-6">
                    <ExpiringCertificationsWidget />
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <OpenIncidentsWidget />
                    <RecentNotificationsWidget />
                </div>
            </DashboardSection>
        </div>
    );
};
