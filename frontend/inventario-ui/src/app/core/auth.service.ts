import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { ApiEndpointFactory } from './patterns/factory/api-endpoint.factory';

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
        localStorage.setItem(TOKEN_KEY, res.token);
        this.token.set(res.token);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.token.set(null);
    void this.router.navigateByUrl('/login');
  }

  isAuthenticated(): boolean {
    return !!this.token();
  }

  private readToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }
}
