import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenStorageService } from '../services/token-storage.service';

export const mfaFlowGuard: CanActivateFn = () => {
  const tokenStorage = inject(TokenStorageService);
  const router = inject(Router);
  const mfaFlow = tokenStorage.getMfaFlow();

  if (mfaFlow?.challengeToken) {
    return true;
  }

  return router.parseUrl('/auth/login');
};
