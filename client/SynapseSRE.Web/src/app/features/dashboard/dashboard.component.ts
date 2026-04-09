import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { IncidentService, Incident } from '../../core/services/incident.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  incidents = signal<Incident[]>([]);
  loading = signal(true);

  get total()    { return this.incidents().length; }
  get pending()  { return this.incidents().filter(i => i.status === 'Pending').length; }
  get resolved() { return this.incidents().filter(i => i.status === 'Resolved').length; }
  get critical() { return this.incidents().filter(i => (i.triageAnalysis?.severityScore ?? 0) >= 4).length; }

  constructor(
    private svc: IncidentService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.svc.getAll().subscribe({
      next: (data) => {
        this.incidents.set(data.sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  scoreClass(score: number) {
    if (score >= 4) return 'score-high';
    if (score === 3) return 'score-medium';
    return 'score-low';
  }

  open(id: string) { this.router.navigate(['/incidents', id]); }
  logout() { this.auth.logout(); }
}