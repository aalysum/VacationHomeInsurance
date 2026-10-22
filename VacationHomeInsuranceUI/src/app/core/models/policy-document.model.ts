export interface PolicyDocument {
  id: string;
  policyId: string;
  version: number;
  status: string;
  contentType: string;
  fileSizeBytes: number;
  sha256Checksum: string | null;
  failureCode: string | null;
  createdAtUtc: string;
  generatedAtUtc: string | null;
}
