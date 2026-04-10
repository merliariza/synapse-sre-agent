import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { IncidentService } from '../../../core/services/incident.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-create-incident',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './create-incident.component.html',
  styleUrl: './create-incident.component.scss'
})
export class CreateIncidentComponent {
  title = '';
  description = '';
  selectedFile = signal<File | null>(null);
  loading = signal(false);
  error = signal('');

  constructor(
    private svc: IncidentService, 
    private router: Router,
    private auth: AuthService 
  ) {}

  onFile(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (file) this.selectedFile.set(file);
  }

  removeFile() { this.selectedFile.set(null); }

submit() {
  this.error.set('');

  if (!this.title.trim() || !this.description.trim()) {
    this.error.set('Title and description are required.');
    return;
  }

  const user = this.auth.getCurrentUser();

  if (!user) {
    this.error.set('Session expired. Please login again.');
    this.auth.logout();
    return;
  }

  this.loading.set(true);

  this.svc.create(
    this.title,
    this.description,
    user.id,
    this.selectedFile() ?? undefined
  ).subscribe({
    next: () => this.router.navigate(['/dashboard']),
    error: (e) => {
      console.error('Error in creation:', e);
      this.error.set(e.error?.error || 'Failed to submit incident.');
      this.loading.set(false);
    }
  });
}
}