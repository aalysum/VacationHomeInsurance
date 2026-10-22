export const ADMIN_ENDPOINTS = {
  pendingClaimsManagers: '/admin/claims-managers/pending',
  approveClaimsManager: (
    applicationId: string
  ) =>
    `/admin/claims-managers/${encodeURIComponent(applicationId)}/approve`,
  rejectClaimsManager: (
    applicationId: string
  ) =>
    `/admin/claims-managers/${encodeURIComponent(applicationId)}/reject`,
  claimsManagerDocument: (
    applicationId: string
  ) =>
    `/admin/claims-managers/${encodeURIComponent(applicationId)}/document`,
  updateUserStatus: (
    userId: string
  ) =>
    `/admin/users/${encodeURIComponent(userId)}/status`,
  customers: '/admin/customers',
  customer: (
    customerId: string
  ) =>
    `/admin/customers/${encodeURIComponent(customerId)}`,
  customerProperties: (
    customerId: string
  ) =>
    `/admin/customers/${encodeURIComponent(customerId)}/properties`
} as const;
