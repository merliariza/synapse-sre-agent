import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TriageAnalysis {
  id: string;
  aiSummary: string;
  severityScore: number;
  suggestedFix: string;
  modelUsed: string;
  processedAt: string;
}

export interface Incident {
  id: string;
  title: string;
  description: string;
  status: string;
  createdAt: string;
  resolvedAt?: string;
  triageAnalysis?: TriageAnalysis;
}

export interface ResolveRequest {
  resolvedBy: string;
  resolutionNotes: string;
  reporterEmail: string;
}

@Injectable({ providedIn: 'root' })
export class IncidentService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<Incident[]> {
    return this.http.get<Incident[]>(`${environment.apiUrl}/incidents`);
  }

  getById(id: string): Observable<Incident> {
    return this.http.get<Incident>(`${environment.apiUrl}/incidents/${id}`);
  }

  create(title: string, description: string, userId: string, logFile?: File): Observable<Incident> {
    const form = new FormData();
    form.append('title', title);
    form.append('description', description);
    form.append('userId', userId); 

    if (logFile) {
      form.append('logFile', logFile);
    }

    return this.http.post<Incident>(`${environment.apiUrl}/incidents`, form);
  }

  resolve(id: string, payload: ResolveRequest): Observable<{ message: string; resolvedAt: string }> {
    return this.http.patch<{ message: string; resolvedAt: string }>(
      `${environment.apiUrl}/incidents/${id}/resolve`, 
      payload
    );
  }
}