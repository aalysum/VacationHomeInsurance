export const POLICY_ENDPOINTS = {
  policies: '/policies',
  policy: (
    policyId: string
  ): string => {
    const id = encodeURIComponent(
      policyId
    );
    return `/policies/${id}`;
  },
  policyDocument: (
    policyId: string,
    documentId: string
  ): string =>
    `/policies/${encodeURIComponent(policyId)}/documents/${encodeURIComponent(documentId)}`
} as const;
