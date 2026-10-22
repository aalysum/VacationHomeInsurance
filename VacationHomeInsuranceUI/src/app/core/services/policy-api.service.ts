// import {
//   HttpClient
// } from '@angular/common/http';


// import {
//   inject,
//   Injectable
// } from '@angular/core';


// import {
//   Observable
// } from 'rxjs';


// import {
//   POLICY_ENDPOINTS
// } from '../constants/policy-endpoints.constants';


// import {
//   PolicyDetails
// } from '../models/policies/policy-details.model';


// import {
//   PolicySummary
// } from '../models/policies/policy-summary.model';


// import {
//   buildApiUrl
// } from '../utils/api-url.util';


// @Injectable({
//   providedIn: 'root'
// })
// export class PolicyApiService {
//   private readonly http =
//     inject(HttpClient);


//   getPolicies():
//     Observable<PolicySummary[]> {
//     return this.http.get<
//       PolicySummary[]
//     >(
//       buildApiUrl(
//         POLICY_ENDPOINTS.policies
//       )
//     );
//   }


//   getPolicy(
//     policyId: string
//   ): Observable<PolicyDetails> {
//     return this.http.get<
//       PolicyDetails
//     >(
//       buildApiUrl(
//         POLICY_ENDPOINTS.policy(
//           policyId
//         )
//       )
//     );
//   }
// }
