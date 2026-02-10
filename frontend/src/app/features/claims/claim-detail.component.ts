import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Apollo, gql } from 'apollo-angular';
import { Subject } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';

const CLAIM_BY_ID = gql`
  query ClaimById($id: ID!) {
    claimById(id: $id) {
      id
      claimNumber
      status
      totalBilled
      totalAllowed
      totalPaid
      receivedDate
      serviceFromDate
      serviceToDate
      rowVersion
      memberId
      providerId
      coverageId
      provider {
        id
        npi
        name
        providerStatus
      }
      lines {
        lineNumber
        cptCode
        units
        billedAmount
        allowedAmount
        paidAmount
        lineStatus
        denialReasonCode
      }
      diagnoses {
        codeSystem
        code
        isPrimary
        lineNumber
      }
      attachments {
        id
        fileName
        contentType
        uploadedAt
        sha256
      }
    }
  }
`;

const UPLOAD_ATTACHMENT = gql`
  mutation UploadAttachment($claimId: ID!, $fileName: String!, $contentType: String!, $base64: String!) {
    uploadClaimAttachment(claimId: $claimId, fileName: $fileName, contentType: $contentType, base64: $base64) {
      id
      claimId
      fileName
      contentType
      storageKey
      sha256
      uploadedAt
    }
  }
`;

@Component({
  selector: 'app-claim-detail',
  standalone: false,
  template: `
    <div class="claim-detail-container">
      <!-- Loading -->
      <app-loading *ngIf="loading" message="Loading claim..."></app-loading>

      <!-- Error -->
      <mat-card *ngIf="error" class="error-card">
        <mat-card-content>
          <mat-icon color="warn">error</mat-icon>
          <span>{{ error }}</span>
          <button mat-button color="primary" (click)="refetch()">Retry</button>
        </mat-card-content>
      </mat-card>

      <ng-container *ngIf="!loading && !error && claim">
        <!-- Header Card -->
        <mat-card class="section-card">
          <mat-card-header>
            <mat-card-title>
              Claim {{ claim.claimNumber }}
              <app-status-badge [status]="claim.status"></app-status-badge>
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Member ID</span>
                <span class="value">{{ claim.memberId }}</span>
              </div>
              <div class="info-item">
                <span class="label">Provider</span>
                <span class="value">{{ claim.provider?.name || claim.providerId }}</span>
              </div>
              <div class="info-item">
                <span class="label">Provider NPI</span>
                <span class="value">{{ claim.provider?.npi || '—' }}</span>
              </div>
              <div class="info-item">
                <span class="label">Received Date</span>
                <span class="value">{{ claim.receivedDate | date:'mediumDate' }}</span>
              </div>
              <div class="info-item">
                <span class="label">Service Dates</span>
                <span class="value">
                  {{ claim.serviceFromDate | date:'shortDate' }} – {{ claim.serviceToDate | date:'shortDate' }}
                </span>
              </div>
              <div class="info-item">
                <span class="label">Coverage ID</span>
                <span class="value">{{ claim.coverageId || '—' }}</span>
              </div>
              <div class="info-item">
                <span class="label">Total Billed</span>
                <span class="value amount">{{ claim.totalBilled | money }}</span>
              </div>
              <div class="info-item">
                <span class="label">Total Allowed</span>
                <span class="value amount">{{ claim.totalAllowed | money }}</span>
              </div>
              <div class="info-item">
                <span class="label">Total Paid</span>
                <span class="value amount">{{ claim.totalPaid | money }}</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Claim Lines -->
        <mat-card class="section-card">
          <mat-card-header>
            <mat-card-title>Claim Lines</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="claim.lines" class="full-width" *ngIf="claim.lines?.length; else noLines">
              <ng-container matColumnDef="lineNumber">
                <th mat-header-cell *matHeaderCellDef>Line #</th>
                <td mat-cell *matCellDef="let line">{{ line.lineNumber }}</td>
              </ng-container>
              <ng-container matColumnDef="cptCode">
                <th mat-header-cell *matHeaderCellDef>CPT Code</th>
                <td mat-cell *matCellDef="let line">{{ line.cptCode }}</td>
              </ng-container>
              <ng-container matColumnDef="units">
                <th mat-header-cell *matHeaderCellDef>Units</th>
                <td mat-cell *matCellDef="let line">{{ line.units }}</td>
              </ng-container>
              <ng-container matColumnDef="billedAmount">
                <th mat-header-cell *matHeaderCellDef>Billed</th>
                <td mat-cell *matCellDef="let line">{{ line.billedAmount | money }}</td>
              </ng-container>
              <ng-container matColumnDef="allowedAmount">
                <th mat-header-cell *matHeaderCellDef>Allowed</th>
                <td mat-cell *matCellDef="let line">{{ line.allowedAmount | money }}</td>
              </ng-container>
              <ng-container matColumnDef="paidAmount">
                <th mat-header-cell *matHeaderCellDef>Paid</th>
                <td mat-cell *matCellDef="let line">{{ line.paidAmount | money }}</td>
              </ng-container>
              <ng-container matColumnDef="lineStatus">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let line">
                  <app-status-badge [status]="line.lineStatus"></app-status-badge>
                </td>
              </ng-container>
              <ng-container matColumnDef="denialReasonCode">
                <th mat-header-cell *matHeaderCellDef>Denial Reason</th>
                <td mat-cell *matCellDef="let line">{{ line.denialReasonCode || '—' }}</td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="lineColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: lineColumns;"></tr>
            </table>
            <ng-template #noLines>
              <p class="no-data">No claim lines.</p>
            </ng-template>
          </mat-card-content>
        </mat-card>

        <!-- Diagnoses -->
        <mat-card class="section-card">
          <mat-card-header>
            <mat-card-title>Diagnoses</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="claim.diagnoses" class="full-width" *ngIf="claim.diagnoses?.length; else noDiagnoses">
              <ng-container matColumnDef="code">
                <th mat-header-cell *matHeaderCellDef>Code</th>
                <td mat-cell *matCellDef="let dx">{{ dx.code }}</td>
              </ng-container>
              <ng-container matColumnDef="codeSystem">
                <th mat-header-cell *matHeaderCellDef>Code System</th>
                <td mat-cell *matCellDef="let dx">{{ dx.codeSystem }}</td>
              </ng-container>
              <ng-container matColumnDef="isPrimary">
                <th mat-header-cell *matHeaderCellDef>Primary</th>
                <td mat-cell *matCellDef="let dx">
                  <mat-icon *ngIf="dx.isPrimary" color="primary">check_circle</mat-icon>
                  <span *ngIf="!dx.isPrimary">—</span>
                </td>
              </ng-container>
              <ng-container matColumnDef="lineNumber">
                <th mat-header-cell *matHeaderCellDef>Line #</th>
                <td mat-cell *matCellDef="let dx">{{ dx.lineNumber || '—' }}</td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="diagnosisColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: diagnosisColumns;"></tr>
            </table>
            <ng-template #noDiagnoses>
              <p class="no-data">No diagnoses.</p>
            </ng-template>
          </mat-card-content>
        </mat-card>

        <!-- Attachments -->
        <mat-card class="section-card">
          <mat-card-header>
            <mat-card-title>Attachments</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div *ngIf="claim.attachments?.length; else noAttachments">
              <mat-list>
                <mat-list-item *ngFor="let att of claim.attachments">
                  <mat-icon matListItemIcon>attach_file</mat-icon>
                  <span matListItemTitle>{{ att.fileName }}</span>
                  <span matListItemLine>{{ att.contentType }} &middot; Uploaded {{ att.uploadedAt | date:'medium' }}</span>
                </mat-list-item>
              </mat-list>
            </div>
            <ng-template #noAttachments>
              <p class="no-data">No attachments.</p>
            </ng-template>

            <!-- Upload -->
            <div class="upload-section">
              <h4>Upload Attachment</h4>
              <input type="file" #fileInput (change)="onFileSelected($event)" [disabled]="uploading">
              <p class="hint">Max file size: 5 MB</p>
              <p *ngIf="uploadError" class="upload-error">{{ uploadError }}</p>
              <button mat-raised-button color="primary"
                      [disabled]="!selectedFile || uploading"
                      (click)="uploadAttachment()">
                <mat-icon>cloud_upload</mat-icon>
                {{ uploading ? 'Uploading...' : 'Upload' }}
              </button>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Appeal (if Denied) -->
        <mat-card *ngIf="claim.status === 'Denied'" class="section-card appeal-card">
          <mat-card-header>
            <mat-card-title>Appeal</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>This claim was denied. You may submit an appeal.</p>
            <button mat-raised-button color="warn" (click)="submitAppeal()">
              <mat-icon>gavel</mat-icon> Submit Appeal
            </button>
          </mat-card-content>
        </mat-card>
      </ng-container>
    </div>
  `,
  styles: [`
    .claim-detail-container {
      padding: 24px;
      max-width: 1200px;
    }
    .section-card {
      margin-bottom: 24px;
    }
    .info-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 16px;
      margin-top: 16px;
    }
    .info-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .info-item .label {
      font-size: 12px;
      color: rgba(0, 0, 0, 0.54);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .info-item .value {
      font-size: 14px;
      font-weight: 500;
    }
    .info-item .value.amount {
      font-family: 'Roboto Mono', monospace;
    }
    .full-width {
      width: 100%;
    }
    .no-data {
      color: rgba(0, 0, 0, 0.54);
      font-style: italic;
      padding: 16px 0;
    }
    .upload-section {
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid rgba(0, 0, 0, 0.12);
    }
    .upload-section h4 {
      margin: 0 0 12px 0;
    }
    .hint {
      font-size: 12px;
      color: rgba(0, 0, 0, 0.54);
      margin: 4px 0 12px 0;
    }
    .upload-error {
      color: #c62828;
      font-size: 13px;
      margin: 4px 0;
    }
    .error-card mat-card-content {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #c62828;
    }
    .appeal-card {
      border-left: 4px solid #c62828;
    }
  `],
})
export class ClaimDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private claimId!: string;
  private queryRef: any;

  claim: any = null;
  loading = false;
  error: string | null = null;

  lineColumns = ['lineNumber', 'cptCode', 'units', 'billedAmount', 'allowedAmount', 'paidAmount', 'lineStatus', 'denialReasonCode'];
  diagnosisColumns = ['code', 'codeSystem', 'isPrimary', 'lineNumber'];

  selectedFile: File | null = null;
  uploading = false;
  uploadError: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private apollo: Apollo,
    private snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      this.claimId = params.get('id')!;
      this.loadClaim();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadClaim(): void {
    this.loading = true;
    this.error = null;

    this.queryRef = this.apollo.watchQuery<any>({
      query: CLAIM_BY_ID,
      variables: { id: this.claimId },
      fetchPolicy: 'network-only',
    });

    this.queryRef.valueChanges.pipe(takeUntil(this.destroy$)).subscribe({
      next: ({ data, loading }: any) => {
        this.loading = loading;
        if (data?.claimById) {
          this.claim = data.claimById;
        }
      },
      error: (err: any) => {
        this.loading = false;
        this.error = err.message || 'Failed to load claim.';
      },
    });
  }

  refetch(): void {
    if (this.queryRef) {
      this.loading = true;
      this.error = null;
      this.queryRef.refetch();
    } else {
      this.loadClaim();
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.uploadError = null;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const maxSize = 5 * 1024 * 1024; // 5 MB
      if (file.size > maxSize) {
        this.uploadError = 'File size exceeds 5 MB limit.';
        this.selectedFile = null;
        return;
      }
      this.selectedFile = file;
    }
  }

  uploadAttachment(): void {
    if (!this.selectedFile) return;

    this.uploading = true;
    this.uploadError = null;

    const reader = new FileReader();
    reader.onload = () => {
      const base64 = (reader.result as string).split(',')[1];
      this.apollo
        .mutate<any>({
          mutation: UPLOAD_ATTACHMENT,
          variables: {
            claimId: this.claimId,
            fileName: this.selectedFile!.name,
            contentType: this.selectedFile!.type || 'application/octet-stream',
            base64,
          },
        })
        .subscribe({
          next: ({ data }) => {
            this.uploading = false;
            this.selectedFile = null;
            this.snackBar.open('Attachment uploaded successfully.', 'OK', { duration: 3000 });
            this.refetch();
          },
          error: (err) => {
            this.uploading = false;
            this.uploadError = err.message || 'Upload failed.';
          },
        });
    };
    reader.onerror = () => {
      this.uploading = false;
      this.uploadError = 'Failed to read file.';
    };
    reader.readAsDataURL(this.selectedFile);
  }

  submitAppeal(): void {
    this.snackBar.open('Appeal submission not yet implemented.', 'OK', { duration: 3000 });
  }
}
