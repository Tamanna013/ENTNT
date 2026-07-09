import React, { useState } from 'react';
import { useUsersQuery, useDeactivateUserMutation } from '../../hooks/useUsers';
import { UsersTable } from '../../components/users/UsersTable';
import { UserFormModal } from '../../components/users/UserFormModal';
import { AssignRolesModal } from '../../components/users/AssignRolesModal';
import { SearchInput } from '../../components/ui/SearchInput';
import { Pagination } from '../../components/ui/Pagination';
import { User } from '../../types/user';
import { Plus } from 'lucide-react';

export const UsersPage: React.FC = () => {
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [sortBy, setSortBy] = useState<string>('createdAt');
  const [sortDescending, setSortDescending] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');

  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isRolesModalOpen, setIsRolesModalOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | undefined>(undefined);

  const { data, isLoading } = useUsersQuery({
    pageNumber,
    pageSize,
    sortBy,
    sortDescending,
    searchTerm
  });

  const deactivateMutation = useDeactivateUserMutation();

  const handleSortChange = (key: string) => {
    if (sortBy === key) {
      setSortDescending(!sortDescending);
    } else {
      setSortBy(key);
      setSortDescending(false);
    }
  };

  const openCreateModal = () => {
    setSelectedUser(undefined);
    setIsFormModalOpen(true);
  };

  const openEditModal = (user: User) => {
    setSelectedUser(user);
    setIsFormModalOpen(true);
  };

  const openRolesModal = (user: User) => {
    setSelectedUser(user);
    setIsRolesModalOpen(true);
  };

  const handleDeactivate = async (user: User) => {
    try {
      await deactivateMutation.mutateAsync(user.id);
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to deactivate user');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">User Management</h1>
          <p className="text-sm text-text-muted mt-1">Manage system users, roles, and access.</p>
        </div>
        <button
          onClick={openCreateModal}
          className="inline-flex items-center justify-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-slate-900 transition-colors"
        >
          <Plus className="h-4 w-4" />
          Create User
        </button>
      </div>

      <div className="flex items-center p-4 bg-slate-900/50 rounded-lg border border-border">
        <SearchInput
          value={searchTerm}
          onChange={(val) => {
            setSearchTerm(val);
            setPageNumber(1); // Reset to first page on search
          }}
          placeholder="Search users by name or email..."
        />
      </div>

      <UsersTable
        users={data?.items || []}
        isLoading={isLoading}
        sortBy={sortBy}
        sortDescending={sortDescending}
        onSortChange={handleSortChange}
        onEdit={openEditModal}
        onAssignRoles={openRolesModal}
        onDeactivate={handleDeactivate}
      />

      {data && (
        <Pagination
          pageNumber={pageNumber}
          totalPages={data.totalPages}
          onPageChange={setPageNumber}
        />
      )}

      {isFormModalOpen && (
        <UserFormModal
          isOpen={isFormModalOpen}
          onClose={() => setIsFormModalOpen(false)}
          user={selectedUser}
        />
      )}

      {isRolesModalOpen && (
        <AssignRolesModal
          isOpen={isRolesModalOpen}
          onClose={() => setIsRolesModalOpen(false)}
          user={selectedUser}
        />
      )}
    </div>
  );
};
