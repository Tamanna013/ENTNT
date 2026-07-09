import React from 'react';
import { Table, Column } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { User } from '../../types/user';

interface UsersTableProps {
  users: User[];
  isLoading: boolean;
  sortBy?: string;
  sortDescending?: boolean;
  onSortChange: (key: string) => void;
  onEdit: (user: User) => void;
  onAssignRoles: (user: User) => void;
  onDeactivate: (user: User) => void;
}

export const UsersTable: React.FC<UsersTableProps> = ({
  users,
  isLoading,
  sortBy,
  sortDescending,
  onSortChange,
  onEdit,
  onAssignRoles,
  onDeactivate
}) => {
  const columns: Column<User>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: false, // Could be sortable if backend supports it
      render: (user) => (
        <div>
          <div className="font-medium text-text-primary">{user.firstName} {user.lastName}</div>
          {user.phoneNumber && <div className="text-xs text-text-muted">{user.phoneNumber}</div>}
        </div>
      )
    },
    {
      key: 'email',
      header: 'Email',
      sortable: true
    },
    {
      key: 'roles',
      header: 'Roles',
      sortable: false,
      render: (user) => (
        <div className="flex flex-wrap gap-1">
          {user.roles.map(role => (
            <Badge key={role} text={role} color="blue" />
          ))}
        </div>
      )
    },
    {
      key: 'status',
      header: 'Status',
      sortable: false,
      render: (user) => (
        <Badge 
          text={user.isActive ? 'Active' : 'Inactive'} 
          color={user.isActive ? 'green' : 'red'} 
        />
      )
    },
    {
      key: 'createdAt',
      header: 'Created At',
      sortable: true,
      render: (user) => new Date(user.createdAt!).toLocaleDateString() // Assuming createdAt comes from BaseEntity
    },
    {
      key: 'actions',
      header: '',
      sortable: false,
      render: (user) => (
        <div className="flex items-center gap-3 justify-end">
          <button 
            onClick={() => onEdit(user)}
            className="text-blue-400 hover:text-blue-300 text-sm font-medium transition-colors"
          >
            Edit
          </button>
          <button 
            onClick={() => onAssignRoles(user)}
            className="text-emerald-400 hover:text-emerald-300 text-sm font-medium transition-colors"
          >
            Roles
          </button>
          {user.isActive && (
            <button 
              onClick={() => {
                if (window.confirm(`Are you sure you want to deactivate ${user.email}?`)) {
                  onDeactivate(user);
                }
              }}
              className="text-rose-400 hover:text-rose-300 text-sm font-medium transition-colors"
            >
              Deactivate
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <Table
      columns={columns}
      data={users}
      sortBy={sortBy}
      sortDescending={sortDescending}
      onSortChange={onSortChange}
      isLoading={isLoading}
      emptyMessage="No users found."
    />
  );
};
