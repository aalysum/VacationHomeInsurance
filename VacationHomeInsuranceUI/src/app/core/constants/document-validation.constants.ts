export const CLAIMS_MANAGER_DOCUMENT_CONFIG = {
  fieldName: 'verificationDocument',
  maxFileSizeBytes: 10 * 1024 * 1024,
  acceptedExtensions: ['.pdf', '.jpg', '.jpeg', '.png'],
  acceptedMimeTypes: ['application/pdf', 'image/jpeg', 'image/png'],
  acceptAttribute: '.pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png'
} as const;
