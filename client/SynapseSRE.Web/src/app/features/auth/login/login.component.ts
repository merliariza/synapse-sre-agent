import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  mode = signal<'login' | 'register'>('login');
  loading = signal(false);
  error = signal('');
  success = signal('');
  username = '';
  email = '';
  password = '';

  constructor(private auth: AuthService, private router: Router) {}

  setMode(m: 'login' | 'register') {
    this.mode.set(m);
    this.error.set('');
    this.success.set('');
  }

  submit() {
    this.error.set('');
    this.success.set('');
    if (!this.username || !this.password) { this.error.set('Please fill all required fields.'); return; }
    this.loading.set(true);

    if (this.mode() === 'login') {
      this.auth.login(this.username, this.password).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: () => { this.error.set('Invalid username or password.'); this.loading.set(false); }
      });
    } else {
      if (!this.email) { this.error.set('Email is required.'); this.loading.set(false); return; }
      this.auth.register(this.username, this.email, this.password).subscribe({
        next: () => {
          this.success.set('Account created! You can sign in now.');
          this.mode.set('login');
          this.loading.set(false);
        },
        error: (e) => { this.error.set(e.error?.message ?? 'Registration failed.'); this.loading.set(false); }
      });
    }
  }
}