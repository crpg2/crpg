export enum Role {
  User = 'User',
  Moderator = 'Moderator',
  Admin = 'Admin',
}

export const SomeRole: Role[] = Object.values(Role)
