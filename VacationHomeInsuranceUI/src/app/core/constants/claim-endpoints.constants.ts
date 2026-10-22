export const CLAIM_ENDPOINTS = {
  claims: '/claims',
  claim: (
    claimId: string
  ): string => {
    const id = encodeURIComponent(
      claimId
    );
    return `/claims/${id}`;
  },
  withdraw: (
    claimId: string
  ): string => {
    const id = encodeURIComponent(
      claimId
    );
    return `/claims/${id}/withdraw`;
  }
} as const;
