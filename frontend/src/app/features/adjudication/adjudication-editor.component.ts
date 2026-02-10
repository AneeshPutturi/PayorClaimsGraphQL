import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Apollo, gql } from 'apollo-angular';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

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
      provider {
        id
        npi
        name
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
    }
  }
`;

const ADJUDICATE_CLAIM = gql`
  mutation AdjudicateClaim($claimId: ID!, $rowVersion: String!, $lines: [AdjudicateLineInput!]!) {
    adjudicateClaim(claimId: $claimId, rowVersion: $rowVersion, lines: $lines) {
      claim {
        id
        claimNumber
        status
        totalAllowed
        totalPaid
        lines {
          lineNumber
          lineStatus
          allowedAmount
          paidAmount
        }
      }
    }
  }
`;

@Component({
  selector: 'app-adjudication-editor',
  standalone: false,
  template: `
    <div class="adjudication-editor-container">
      <div class="page-header">
        <h2>Adjudicate Claim</h2>
        <button mat-stroked-button routerLink="/adjudication">
          <mat-icon>arrow_back</mat-icon> Back to Queue
        </button>
      </div>

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
        <!-- Claim Summary -->
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

        <!-- Adjudication Lines -->
        <mat-card class="section-card">
          <mat-card-header>
            <mat-card-title>Line Adjudication</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <form [formGroup]="adjForm">
              <table mat-table [dataSource]="linesArray.controls" class="full-width adjudication-table">
                <ng-container matColumnDef="lineNumber">
                  <th mat-header-cell *matHeaderCellDef>Line #</th>
                  <td mat-cell *matCellDef="let ctrl; let i = index">
                    {{ claim.lines[i]?.lineNumber }}
                  </td>
                </ng-container>

                <ng-container matColumnDef="cptCode">
                  <th mat-header-cell *matHeaderCellDef>CPT Code</th>
                  <td mat-cell *matCellDef="let ctrl; let i = index">
                    {{ claim.lines[i]?.cptCode }}
                  </td>
                </ng-container>

                <ng-container matColumnDef="billedAmount">
                  <th mat-header-cell *matHeaderCellDef>Billed</th>
                  <td mat-cell *matCellDef="let ctrl; let i = index">
                    {{ claim.lines[i]?.billedAmount | money }}
                  </td>
                </ng-container>

                <ng-container matColumnDef="allowedAmount">
                  <th mat-header-cell *matHeaderCellDef>Allowed</th>
                  <td mat-cell *matCellDef="let ctrl; let i = index" [formGroupName]="i">
                    <mat-form-field appearance="outline" class="table-field">
                      <input matInput type="number" formControlName="allowedAmount" min="0" step="0.01">
                    </mat-form-field>
                  </td>
                </ng-container>

                <ng-container matColumnDef="paidAmount">
                  <th mat-header-cell *matHeaderCellDef>Paid</th>
                  <td mat-cell *matCellDef="let ctrl; let i = index" [formGroupName]="i">
                    <mat-form-field appearance="outline" class="table-field">
                      <input matInput type="number" formControlName="paidAmount" min="0" step="0.01">
                    </mat-form-field>
                  </td>
                </ng-container>

                <ng-container matColumnDef="lineStatus">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let ctrl; let i = index" [formGroupName]="i">
                    <mat-form-field appearance="outline" class="table-field">
                      <mat-select formControlName="lineStatus">
                        <mat-option value="Paid">Paid</mat-option>
                        <mat-option value="Denied">Denied</mat-option>
                      </mat-select>
                    </mat-form-field>
                  </td>
                </ng-container>

                <ng-container matColumnDef="denialReasonCode">
                  <th mat-header-cell *matHeaderCellDef>Denial Reason</th>
                  <td mat-cell *matCellDef="let ctrl; let i = index" [formGroupName]="i">
                    <mat-form-field appearance="outline" class="table-field"
                                    *ngIf="ctrl.get('lineStatus')?.value === 'Denied'">
                      <input matInput formControlName="denialReasonCode" placeholder="Reason code">
                    </mat-form-field>
                    <span *ngIf="ctrl.get('lineStatus')?.value !== 'Denied'">—</span>
                  </td>
                </ng-container>

                <tr mat-header-row *matHeaderRowDef="adjColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: adjColumns;"></tr>
              </table>
            </form>

            <p *ngIf="submitError" class="submit-error">{{ submitError }}</p>

            <div class="submit-actions">
              <button mat-raised-button color="primary"
                      [disabled]="submitting || adjForm.invalid"
                      (click)="submitAdjudication()">
                <mat-icon>gavel</mat-icon>
                {{ submitting ? 'Submitting...' : 'Submit Adjudication' }}
              </button>
            </div>
          </mat-card-content>
        </mat-card>
      </ng-container>
    </div>
  `,
  styles: [`
    .adjudication-editor-container {
      padding: 24px;
      max-width: 1200px;
    }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }
    .page-header h2 {
      margin: 0;
      font-size: 24px;
      font-weight: 500;
    }
    .section-card {
      margin-bottom: 24px;
    }
    .info-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
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
    .adjudication-table {
      margin-top: 8px;
    }
    .table-field {
      width: 120px;
    }
    .table-field .mat-mdc-form-field-subscript-wrapper {
      display: none;
    }
    .submit-actions {
      display: flex;
      justify-content: flex-end;
      padding-top: 16px;
    }
    .submit-error {
      color: #c62828;
      font-weight: 500;
      margin-top: 12px;
    }
    .error-card mat-card-content {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #c62828;
    }
  `],
})
export class AdjudicationEditorComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private claimId!: string;
  private queryRef: any;

  claim: any = null;
  rowVersion: string = '';
  loading = false;
  error: string | null = null;
  submitting = false;
  submitError: string | null = null;

  adjForm!: FormGroup;
  adjColumns = ['lineNumber', 'cptCode', 'billedAmount', 'allowedAmount', 'paidAmount', 'lineStatus', 'denialReasonCode'];

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private apollo: Apollo,
    private snackBar: MatSnackBar,
  ) {
    this.adjForm = this.fb.group({
      lines: this.fb.array([]),
    });
  }

  get linesArray(): FormArray {
    return this.adjForm.get('lines') as FormArray;
  }

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
          this.rowVersion = data.claimById.rowVersion;
          this.buildLinesForm(data.claimById.lines);
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

  private buildLinesForm(lines: any[]): void {
    const arr = this.fb.array(
      lines.map((line: any) =>
        this.fb.group({
          lineNumber: [line.lineNumber],
          allowedAmount: [line.allowedAmount || 0, [Validators.required, Validators.min(0)]],
          paidAmount: [line.paidAmount || 0, [Validators.required, Validators.min(0)]],
          lineStatus: [line.lineStatus || 'Paid', Validators.required],
          denialReasonCode: [line.denialReasonCode || ''],
        })
      )
    );
    this.adjForm.setControl('lines', arr);
  }

  submitAdjudication(): void {
    this.submitting = true;
    this.submitError = null;

    const lines = this.linesArray.value.map((line: any) => ({
      lineNumber: line.lineNumber,
      allowedAmount: line.allowedAmount,
      paidAmount: line.paidAmount,
      lineStatus: line.lineStatus,
      denialReasonCode: line.lineStatus === 'Denied' ? line.denialReasonCode : null,
    }));

    this.apollo
      .mutate<any>({
        mutation: ADJUDICATE_CLAIM,
        variables: {
          claimId: this.claimId,
          rowVersion: this.rowVersion,
          lines,
        },
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ data }) => {
          this.submitting = false;
          const result = data.adjudicateClaim.claim;
          this.claim = { ...this.claim, ...result };
          this.snackBar.open(
            `Claim ${result.claimNumber} adjudicated — Status: ${result.status}`,
            'OK',
            { duration: 4000 },
          );
        },
        error: (err) => {
          this.submitting = false;

          // Handle concurrency conflict
          const gqlErrors = err.graphQLErrors || [];
          const concurrencyError = gqlErrors.find(
            (e: any) => e.extensions?.code === 'CONCURRENCY_CONFLICT'
          );

          if (concurrencyError) {
            const ref = this.snackBar.open(
              'This claim was modified by another user. Please reload.',
              'Reload',
              { duration: 10000 },
            );
            ref.onAction().subscribe(() => {
              this.refetch();
            });
          } else {
            this.submitError = err.message || 'Failed to submit adjudication.';
          }
        },
      });
  }
}
