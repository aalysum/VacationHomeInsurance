import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenStorageService } from '../services/token-storage.service';

export const claimsManagerReapplicationGuard: CanActivateFn = () => {
  const tokenStorage = inject(TokenStorageService);
  const router = inject(Router);
  const flow = tokenStorage.getClaimsManagerFlow();

  if (!flow?.applicationToken) {
    return router.parseUrl('/auth/login');
  }

  if (flow.status.toLowerCase() !== 'rejected') {
    return router.parseUrl('/auth/claims-manager/application-status');
  }

  return true;
};
