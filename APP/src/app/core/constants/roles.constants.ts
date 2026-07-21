export const ROLES = {
  Admin: 'Admin',
  ProjectManager: 'ProjectManager',
  Developer: 'Developer',
  QA: 'QA',
  Viewer: 'Viewer'
} as const;

export type AppRole = (typeof ROLES)[keyof typeof ROLES];
