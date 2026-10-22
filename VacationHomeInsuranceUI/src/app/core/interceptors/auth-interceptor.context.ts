import {
  HttpContextToken
} from '@angular/common/http';

export const AUTH_RETRY_ATTEMPTED =
  new HttpContextToken<boolean>(
    () => false
  );
