import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    usuario: ['', Validators.required],
    clave: ['', Validators.required]
  });

  errorMessage = '';
  loading = false;

  submit(): void {
    this.errorMessage = '';
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { usuario, clave } = this.form.getRawValue();
    this.loading = true;
    this.auth.login(usuario, clave).subscribe({
      next: () => void this.router.navigateByUrl('/inventario'),
      error: (err) => {
        this.loading = false;
        const msg = err?.error?.message ?? 'No se pudo iniciar sesión.';
        this.errorMessage = typeof msg === 'string' ? msg : 'Credenciales inválidas.';
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}
