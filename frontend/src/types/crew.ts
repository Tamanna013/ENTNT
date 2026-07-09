import { PaginationQuery } from './pagination';

export interface CrewMember {
  id: string;
  shipId: string | null;
  shipName: string | null;
  firstName: string;
  lastName: string;
  rank: string;
  status: string;
  nationality: string;
  dateOfBirth: string;
  licenseNumber: string;
  hireDate: string;
  contactEmail: string | null;
  contactPhone: string | null;
  createdAt: string;
}

export interface CreateCrewMemberPayload {
  firstName: string;
  lastName: string;
  rank: string;
  nationality: string;
  dateOfBirth: string;
  licenseNumber: string;
  hireDate: string;
  contactEmail?: string;
  contactPhone?: string;
}

export interface UpdateCrewMemberPayload {
  firstName: string;
  lastName: string;
  rank: string;
  status: string;
  nationality: string;
  contactEmail?: string;
  contactPhone?: string;
}

export interface CrewQuery extends PaginationQuery {
  shipId?: string;
  status?: string;
  rank?: string;
  unassigned?: boolean;
}

export interface CrewCertification {
  id: string;
  crewMemberId: string;
  certificationName: string;
  expiryDate: string;
  isExpired: boolean;
  downloadUrl: string;
  fileName: string;
  createdAt: string;
}
