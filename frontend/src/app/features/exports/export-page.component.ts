import { Component, OnDestroy } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

const REQUEST_EXPORT_MUTATION = gql`
  mutation RequestMemberClaimsExport($memberId: UUID!) {
    requestMemberClaimsExport(memberId: $memberId) {
      jobId
      status
    }
  }
`;

const EXPORT_JOB_QUERY = gql`
  query ExportJob($jobId: UUID!) {
    exportJob(jobId: $jobId) {
      jobId
      status
      downloadTokenOnce
    }
  }
`;

@Component({
  selector: 'app-export-page',
  standalone: false,
  template: `
    <div class="export-container">
      <h2 class="page-title">Export Member Claims</h2>

      <mat-card class="export-card">
        <mat-card-content>
          <mat-form-field appearance="outline" class="member-field">
            <mat-label>Member ID</mat-label>
            <input matInput [(ngModel)]="memberId" placeholder="Enter member UUID" />
          </mat-form-field>

          <button mat-raised-button
                  color="primary"
                  (click)="requestExport()"
                  [disabled]="!memberId.trim() || loading">
            <mat-icon>file_download</mat-icon>
            Request Export
          </button>
        </mat-card-content>
      </mat-card>

      @if (loading) {
        <app-loading message="Processing export…"></app-loading>
      }

      @if (error) {
        <mat-card class="error-card">
          <mat-card-content>
            <div class="error-content">
              <mat-icon color="warn">error_outline</mat-icon>
              <span>{{ error }}</span>
            </div>
          </mat-card-content>
        </mat-card>
      }

      @if (jobId) {
        <mat-card class="status-card">
          <mat-card-header>
            <mat-card-title>Export Job</mat-card-title>
            <mat-card-subtitle>Job ID: {{ jobId }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <div class="status-row">
              <span class="label">Status:</span>
              <app-status-badge [status]="jobStatus"></app-status-badge>
            </div>

            @if (jobStatus === 'Ready' && downloadToken) {
              <div class="token-section">
                <mat-form-field appearance="outline" class="token-field">
                  <mat-label>Download Token</mat-label>
                  <input matInput [value]="downloadToken" readonly />
                </mat-form-field>
                <button mat-stroked-button (click)="copyToken()" class="copy-btn">
                  <mat-icon>content_copy</mat-icon>
                  {{ copied ? 'Copied!' : 'Copy' }}
                </button>
              </div>

              <p class="warning-text">
                <mat-icon class="warning-icon">warning</mat-icon>
                Token can only be used once
              </p>

              <a [href]="downloadUrl"
                 target="_blank"
                 mat-raised-button
                 color="accent"
                 class="download-btn">
                <mat-icon>cloud_download</mat-icon>
                Download Export
              </a>
            }
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .export-container {
      max-width: 800px;
      margin: 0 auto;
    }

    .page-title {
      margin: 0 0 24px 0;
      font-size: 24px;
      font-weight: 500;
    }

    .export-card {
      margin-bottom: 24px;
    }

    .member-field {
      width: 100%;
      margin-bottom: 8px;
    }

    .error-card {
      margin-bottom: 24px;
    }

    .error-content {
      display: flex;
      align-items: center;
      gap: 12px;
      color: rgba(0, 0, 0, 0.87);
    }

    .status-card {
      margin-bottom: 24px;
    }

    .status-row {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
    }

    .label {
      font-weight: 500;
    }

    .token-section {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      margin-bottom: 8px;
    }

    .token-field {
      flex: 1;
    }

    .copy-btn {
      margin-top: 8px;
    }

    .warning-text {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #e65100;
      font-size: 0.875rem;
      margin-bottom: 16px;
    }

    .warning-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    .download-btn {
      margin-top: 8px;
    }
  `],
})
export class ExportPageComponent implements OnDestroy {
  memberId = '';
  jobId: string | null = null;
  jobStatus = '';
  downloadToken: string | null = null;
  loading = false;
  error: string | null = null;
  copied = false;

  private destroy$ = new Subject<void>();
  private pollTimer: ReturnType<typeof setInterval> | null = null;

  constructor(private apollo: Apollo) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stopPolling();
  }

  get downloadUrl(): string {
    return (
      environment.exportsBaseUrl +
      '/exports/' +
      this.jobId +
      '/download?token=' +
      encodeURIComponent(this.downloadToken || '')
    );
  }

  requestExport(): void {
    const id = this.memberId.trim();
    if (!id) return;

    this.loading = true;
    this.error = null;
    this.jobId = null;
    this.jobStatus = '';
    this.downloadToken = null;
    this.copied = false;
    this.stopPolling();

    this.apollo
      .mutate<{ requestMemberClaimsExport: { jobId: string; status: string } }>({
        mutation: REQUEST_EXPORT_MUTATION,
        variables: { memberId: id },
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ data }) => {
          if (data?.requestMemberClaimsExport) {
            this.jobId = data.requestMemberClaimsExport.jobId;
            this.jobStatus = data.requestMemberClaimsExport.status;
            this.loading = false;
            this.startPolling();
          }
        },
        error: (err) => {
          this.error = err?.message || 'Failed to request export.';
          this.loading = false;
        },
      });
  }

  copyToken(): void {
    if (this.downloadToken) {
      navigator.clipboard.writeText(this.downloadToken);
      this.copied = true;
      setTimeout(() => (this.copied = false), 2000);
    }
  }

  private startPolling(): void {
    this.pollTimer = setInterval(() => this.pollJob(), 3000);
  }

  private stopPolling(): void {
    if (this.pollTimer) {
      clearInterval(this.pollTimer);
      this.pollTimer = null;
    }
  }

  private pollJob(): void {
    if (!this.jobId) return;

    this.apollo
      .query<{ exportJob: { jobId: string; status: string; downloadTokenOnce: string | null } }>({
        query: EXPORT_JOB_QUERY,
        variables: { jobId: this.jobId },
        fetchPolicy: 'network-only',
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ data }) => {
          if (data?.exportJob) {
            this.jobStatus = data.exportJob.status;
            if (data.exportJob.status === 'Ready') {
              this.downloadToken = data.exportJob.downloadTokenOnce;
              this.stopPolling();
            }
          }
        },
        error: () => {
          this.stopPolling();
          this.error = 'Failed to check export status.';
        },
      });
  }
}
