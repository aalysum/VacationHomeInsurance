export const ADMIN_ENDPOINTS = {
  pendingClaimsManagers: '/admin/claims-managers/pending',
  activeClaimsManagers: '/admin/claims-managers/active',
  approveClaimsManager: (
    applicationId: string
  ): string => {
    const id = encodeURIComponent(
      applicationId.trim()
    );
    return (
      `/admin/claims-managers/${id}` +
      '/approve'
    );
  },
  rejectClaimsManager: (
    applicationId: string
  ): string => {
    const id = encodeURIComponent(
      applicationId.trim()
    );
    return (
      `/admin/claims-managers/${id}` +
      '/reject'
    );
  },
  claimsManagerDocument: (
    applicationId: string
  ): string => {
    const id = encodeURIComponent(
      applicationId.trim()
    );
    return (
      `/admin/claims-managers/${id}` +
      '/document'
    );
  },
  updateUserStatus: (
    userId: string
  ): string => {
    const id = encodeURIComponent(
      userId.trim()
    );
    return `/admin/users/${id}/status`;
  },
  customers: '/admin/customers',
  customer: (
    customerId: string
  ): string => {
    const id = encodeURIComponent(
      customerId.trim()
    );
    return `/admin/customers/${id}`;
  },
  customerProperties: (
    customerId: string
  ): string => {
    const id = encodeURIComponent(
      customerId.trim()
    );
    return (
      `/admin/customers/${id}` +
      '/properties'
    );
  },
  quotes: '/admin/quotes',
  quote: (
    quoteId: string
  ): string => {
    const id = encodeURIComponent(
      quoteId.trim()
    );
    return `/admin/quotes/${id}`;
  },
  approveQuote: (
    quoteId: string
  ): string => {
    const id = encodeURIComponent(
      quoteId.trim()
    );
    return `/admin/quotes/${id}/approve`;
  },
  rejectQuote: (
    quoteId: string
  ): string => {
    const id = encodeURIComponent(
      quoteId.trim()
    );
    return `/admin/quotes/${id}/reject`;
  },
  invalidateQuote: (
    quoteId: string
  ): string => {
    const id = encodeURIComponent(
      quoteId.trim()
    );
    return (
      `/admin/quotes/${id}` +
      '/invalidate'
    );
  },
  products: '/admin/products',
  product: (
    productId: string
  ): string => {
    const id = encodeURIComponent(
      productId.trim()
    );
    return `/admin/products/${id}`;
  },
  updateProductStatus: (
    productId: string
  ): string => {
    const id = encodeURIComponent(
      productId.trim()
    );
    return (
      `/admin/products/${id}` +
      '/status'
    );
  },
  updateCoverageLimit: (
    productId: string,
    coverageId: string
  ): string => {
    const normalizedProductId = encodeURIComponent(
      productId.trim()
    );
    const normalizedCoverageId = encodeURIComponent(
      coverageId.trim()
    );
    return (
      `/admin/products/${normalizedProductId}` +
      `/coverages/${normalizedCoverageId}` +
      '/limit'
    );
  },
  updateCoverageCharge: (
    productId: string,
    coverageId: string
  ): string => {
    const normalizedProductId = encodeURIComponent(
      productId.trim()
    );
    const normalizedCoverageId = encodeURIComponent(
      coverageId.trim()
    );
    return (
      `/admin/products/${normalizedProductId}` +
      `/coverages/${normalizedCoverageId}` +
      '/charge'
    );
  },
  pricing: '/admin/pricing',
  policies: '/admin/policies',
  policy: (
    policyId: string
  ): string => {
    const id = encodeURIComponent(
      policyId.trim()
    );
    return `/admin/policies/${id}`;
  },
  policyDocument: (
    policyId: string,
    documentId: string
  ): string =>
    `/admin/policies/${encodeURIComponent(policyId.trim())}/documents/${encodeURIComponent(documentId.trim())}`,
  retryPolicyDocument: (
    policyId: string
  ): string =>
    `/admin/policies/${encodeURIComponent(policyId.trim())}/documents/retry`,
  claimsDashboard: '/admin/claims/dashboard',
  claims: '/admin/claims',
  claim: (
    claimId: string
  ): string => {
    const id = encodeURIComponent(
      claimId.trim()
    );
    return `/admin/claims/${id}`;
  },
  assignClaimsManager: (
    claimId: string
  ): string => {
    const id = encodeURIComponent(
      claimId.trim()
    );
    return (
      `/admin/claims/${id}` +
      '/assignment'
    );
  }
} as const;
