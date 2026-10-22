export type ClaimsManagerNavigationIcon =
  | 'overview'
  | 'claims'
  | 'profile'
  | 'security';

export interface ClaimsManagerNavigationItem {
  label: string;
  description: string;
  icon: ClaimsManagerNavigationIcon;
  route: string;
  exact: boolean;
}

export const CLAIMS_MANAGER_NAVIGATION_ITEMS: readonly ClaimsManagerNavigationItem[] = [
  {
    label: 'Overview',
    description: 'Assigned Claim activity',
    icon: 'overview',
    route: '/claims-manager/dashboard',
    exact: true
  },
  {
    label: 'Assigned Claims',
    description: 'Review and process Claims',
    icon: 'claims',
    route: '/claims-manager/claims',
    exact: false
  },
  {
    label: 'Profile',
    description: 'Claims Manager profile',
    icon: 'profile',
    route: '/claims-manager/profile',
    exact: true
  },
  {
    label: 'Security',
    description: 'Password and MFA settings',
    icon: 'security',
    route: '/account/security/mfa',
    exact: false
  }
];
