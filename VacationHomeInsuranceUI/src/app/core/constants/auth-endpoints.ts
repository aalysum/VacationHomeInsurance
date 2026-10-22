export const AUTH_ENDPOINTS = {
  registerCustomer: '/auth/customers/register',
  registerClaimsManager: '/auth/claims-managers/register',
  login: '/auth/login',
  refresh: '/auth/refresh',
  logout: '/auth/logout',
  forgotPassword: '/auth/forgot-password',
  resetPassword: '/auth/reset-password',
  changePassword: '/auth/change-password',
  claimsManagerApplicationStatus: '/auth/claims-managers/application-status',
  reapplyClaimsManager: '/auth/claims-managers/reapply',
  beginMfaSetup: '/auth/mfa/setup',
  confirmMfaSetup: '/auth/mfa/confirm',
  verifyMfa: '/auth/mfa/verify',
  disableMfa: '/auth/mfa/disable'
} as const;
