export type AdminNavigationIcon =
  | 'overview'
  | 'applications'
  | 'quotes'
  | 'products'
  | 'pricing'
  | 'users'
  | 'operations'
  | 'reports'
  | 'security';

export interface AdminNavigationItem {
  label: string;
  description: string;
  route: string | null;
  icon: AdminNavigationIcon;
  available: boolean;
}

export const ADMIN_NAVIGATION_ITEMS: readonly AdminNavigationItem[] = [
  {
    label: 'Overview',
    description: 'Administrative summary',
    route: '/admin/dashboard',
    icon: 'overview',
    available: true
  },
  {
    label: 'Claims Manager Applications',
    description: 'Review pending applicants',
    route: '/admin/claims-managers/pending',
    icon: 'applications',
    available: true
  },
  {
    label: 'Quote Approvals',
    description: 'Review accepted Quotes',
    route: '/admin/quotes',
    icon: 'quotes',
    available: true
  },
  {
    label: 'Products & Coverages',
    description: 'Manage insurance Products',
    route: '/admin/products',
    icon: 'products',
    available: true
  },
  {
    label: 'Pricing Configuration',
    description: 'Manage future Pricing',
    route: '/admin/pricing',
    icon: 'pricing',
    available: true
  },
  {
    label: 'User Management',
    description: 'Manage Customer accounts',
    route: '/admin/customers',
    icon: 'users',
    available: true
  },
  {
    label: 'Operations',
    description: 'Monitor Policies, Claims, and assignments',
    route: '/admin/operations',
    icon: 'operations',
    available: true
  },
  {
    label: 'Reports',
    description: 'View operational reports',
    route: null,
    icon: 'reports',
    available: false
  },
  {
    label: 'Security Settings',
    description: 'Password and MFA',
    route: '/account/security/change-password',
    icon: 'security',
    available: true
  }
];
