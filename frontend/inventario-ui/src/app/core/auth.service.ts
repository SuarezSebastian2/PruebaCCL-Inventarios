import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { ApiEndpointFactory } from './patterns/factory/api-endpoint.factory';

/** sessionStorage reduce persistencia del JWT frente a XSS respecto a localStorage (se pierde al cerrar la pestaña). */
const TOKEN_KEY = 'ccl_inv_token';

export interface LoginResponse {
  token: string;
  expiresInSeconds: number;
  tokenType: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly endpoints = inject(ApiEndpointFactory);

  readonly token = signal<string | null>(this.readToken());

  login(usuario: string, clave: string) {
    return this.http.post<LoginResponse>(this.endpoints.login(), { usuario, clave }).pipe(
      tap((res) => {
        sessionStorage.setItem(TOKEN_KEY, res.token);
        this.token.set(res.token);
      })
    );
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    this.token.set(null);
    void this.router.navigateByUrl('/login');
  }

  isAuthenticated(): boolean {
    return !!this.token();
  }

  private readToken(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }
}
