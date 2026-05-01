import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from './environment';

const TOKEN_KEY = 'ccl_inv_token';

export interface LoginResponse {
  token: string;
  expiresInSeconds: number;
  tokenType: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly token = signal<string | null>(this.readToken());

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  login(usuario: string, clave: string) {
    const url = `${environment.apiBaseUrl}/auth/login`;
    return this.http.post<LoginResponse>(url, { usuario, clave }).pipe(
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
