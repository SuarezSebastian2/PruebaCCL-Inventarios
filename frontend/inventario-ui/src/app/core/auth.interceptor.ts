import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { ApiEndpointFactory } from './patterns/factory/api-endpoint.factory';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const endpoints = inject(ApiEndpointFactory);
  const token = auth.token();
  const isApi = req.url.startsWith(endpoints.origin());
  if (token && isApi) {
    const cloned = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
    return next(cloned);
  }
  return next(req);
};
