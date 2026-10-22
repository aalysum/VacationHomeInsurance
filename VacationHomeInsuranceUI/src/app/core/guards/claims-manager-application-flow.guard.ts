import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenStorageService } from '../services/token-storage.service';

export const claimsManagerApplicationFlowGuard: CanActivateFn = () => {
  const tokenStorage = inject(TokenStorageService);
  const router = inject(Router);
  const flow = tokenStorage.getClaimsManagerFlow();

  if (flow?.applicationToken) {
    return true;
  }

  return router.parseUrl('/auth/login');
};
