import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IncidentService, Incident } from '../../../core/services/incident.service';

@Component({
  selector: 'app-incident-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './incident-detail.component.html',
  styleUrl: './incident-detail.component.scss'
})
export class IncidentDetailComponent implements OnInit {
  incident = signal<Incident | null>(null);
  loading = signal(true);
  resolving = signal(false);
  resolveError = signal('');
  resolveSuccess = signal('');
  resolvedBy = 'SRE Team';
  resolutionNotes = '';
  reporterEmail = '';

  constructor(private route: ActivatedRoute, private svc: IncidentService) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.svc.getById(id).subscribe({
      next: (inc) => { this.incident.set(inc); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  scoreClass(score: number) {
    if (score >= 4) return 'score-high';
    if (score === 3) return 'score-medium';
    return 'score-low';
  }

  resolve() {
    this.resolveError.set('');
    this.resolveSuccess.set('');
    if (!this.resolvedBy.trim()) { this.resolveError.set('Resolved by is required.'); return; }
    this.resolving.set(true);
    this.svc.resolve(this.incident()!.id, {
      resolvedBy: this.resolvedBy,
      resolutionNotes: this.resolutionNotes,
      reporterEmail: this.reporterEmail
    }).subscribe({
      next: (res) => {
        this.resolveSuccess.set('✅ Incident resolved. Reporter notified.');
        this.incident.update(i => i ? { ...i, status: 'Resolved', resolvedAt: res.resolvedAt } : i);
        this.resolving.set(false);
      },
      error: () => {
        this.resolveError.set('Failed to resolve incident. Try again.');
        this.resolving.set(false);
      }
    });
  }
}