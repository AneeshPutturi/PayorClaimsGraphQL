import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Apollo, gql } from 'apollo-angular';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

const SUBMIT_CLAIM = gql`
  mutation SubmitClaim($input: ClaimSubmissionInput!) {
    submitClaim(input: $input) {
      claim {
        id
        claimNumber
        status
        totalBilled
        receivedDate
      }
      alreadyExisted
    }
  }
`;

@Component({
  selector: 'app-claim-submit',
  standalone: false,
  template: `
    <div class="claim-submit-container">
      <div class="page-header">
        <h2>Submit New Claim</h2>
        <button mat-stroked-button routerLink="/claims">
          <mat-icon>arrow_back</mat-icon> Back to Claims
        </button>
      </div>

      <!-- Success Result -->
      <mat-card *ngIf="submitResult" class="result-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon color="primary">check_circle</mat-icon>
            Claim Submitted
          </mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="result-grid">
            <div class="result-item">
              <span class="label">Claim Number</span>
              <span class="value">{{ submitResult.claim.claimNumber }}</span>
            </div>
            <div class="result-item">
              <span class="label">Status</span>
              <app-status-badge [status]="submitResult.claim.status"></app-status-badge>
            </div>
            <div class="result-item">
              <span class="label">Total Billed</span>
              <span class="value">{{ submitResult.claim.totalBilled | money }}</span>
            </div>
            <div class="result-item" *ngIf="submitResult.alreadyExisted">
              <mat-chip color="warn" highlighted>Duplicate — Already Existed</mat-chip>
            </div>
          </div>
          <div class="result-actions">
            <button mat-raised-button color="primary" [routerLink]="['/claims', submitResult.claim.id]">
              View Claim
            </button>
            <button mat-stroked-button (click)="resetForm()">Submit Another</button>
          </div>
        </mat-card-content>
      </mat-card>

      <!-- Stepper Form -->
      <mat-stepper *ngIf="!submitResult" linear #stepper>

        <!-- Step 1: Claim Header -->
        <mat-step [stepControl]="headerForm" label="Claim Header">
          <form [formGroup]="headerForm">
            <div class="step-content">
              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>Member ID</mat-label>
                  <input matInput formControlName="memberId" placeholder="Enter member ID">
                  <mat-error *ngIf="headerForm.get('memberId')?.hasError('required')">Required</mat-error>
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Provider ID / NPI</mat-label>
                  <input matInput formControlName="providerId" placeholder="Enter provider ID or NPI">
                  <mat-error *ngIf="headerForm.get('providerId')?.hasError('required')">Required</mat-error>
                </mat-form-field>
              </div>

              <div class="form-row">
                <mat-form-field appearance="outline">
                  <mat-label>Service From</mat-label>
                  <input matInput [matDatepicker]="serviceFrom" formControlName="serviceFrom">
                  <mat-datepicker-toggle matIconSuffix [for]="serviceFrom"></mat-datepicker-toggle>
                  <mat-datepicker #serviceFrom></mat-datepicker>
                  <mat-error *ngIf="headerForm.get('serviceFrom')?.hasError('required')">Required</mat-error>
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Service To</mat-label>
                  <input matInput [matDatepicker]="serviceTo" formControlName="serviceTo">
                  <mat-datepicker-toggle matIconSuffix [for]="serviceTo"></mat-datepicker-toggle>
                  <mat-datepicker #serviceTo></mat-datepicker>
                  <mat-error *ngIf="headerForm.get('serviceTo')?.hasError('required')">Required</mat-error>
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Received Date</mat-label>
                  <input matInput [matDatepicker]="receivedDate" formControlName="receivedDate">
                  <mat-datepicker-toggle matIconSuffix [for]="receivedDate"></mat-datepicker-toggle>
                  <mat-datepicker #receivedDate></mat-datepicker>
                  <mat-error *ngIf="headerForm.get('receivedDate')?.hasError('required')">Required</mat-error>
                </mat-form-field>
              </div>
            </div>

            <div class="step-actions">
              <button mat-raised-button color="primary" matStepperNext [disabled]="headerForm.invalid">Next</button>
            </div>
          </form>
        </mat-step>

        <!-- Step 2: Diagnoses -->
        <mat-step [stepControl]="diagnosesForm" label="Diagnoses">
          <form [formGroup]="diagnosesForm">
            <div class="step-content">
              <p class="step-hint">Add at least one diagnosis. Mark one as primary.</p>
              <div formArrayName="diagnoses">
                <div *ngFor="let dx of diagnosesArray.controls; let i = index" [formGroupName]="i" class="inline-row">
                  <mat-form-field appearance="outline" class="narrow-field">
                    <mat-label>Code System</mat-label>
                    <mat-select formControlName="codeSystem">
                      <mat-option value="ICD-10">ICD-10</mat-option>
                      <mat-option value="ICD-9">ICD-9</mat-option>
                    </mat-select>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Code</mat-label>
                    <input matInput formControlName="code" placeholder="e.g. J06.9">
                    <mat-error *ngIf="dx.get('code')?.hasError('required')">Required</mat-error>
                  </mat-form-field>

                  <mat-checkbox formControlName="isPrimary" color="primary">Primary</mat-checkbox>

                  <button mat-icon-button color="warn" (click)="removeDiagnosis(i)"
                          [disabled]="diagnosesArray.length <= 1" matTooltip="Remove diagnosis">
                    <mat-icon>delete</mat-icon>
                  </button>
                </div>
              </div>

              <button mat-stroked-button color="primary" (click)="addDiagnosis()">
                <mat-icon>add</mat-icon> Add Diagnosis
              </button>
            </div>

            <div class="step-actions">
              <button mat-stroked-button matStepperPrevious>Back</button>
              <button mat-raised-button color="primary" matStepperNext
                      [disabled]="diagnosesArray.length === 0 || diagnosesForm.invalid">Next</button>
            </div>
          </form>
        </mat-step>

        <!-- Step 3: Claim Lines -->
        <mat-step [stepControl]="linesForm" label="Claim Lines">
          <form [formGroup]="linesForm">
            <div class="step-content">
              <p class="step-hint">Add one or more claim lines with CPT codes and billing amounts.</p>
              <div formArrayName="lines">
                <mat-card *ngFor="let line of linesArray.controls; let i = index" class="line-card" [formGroupName]="i">
                  <mat-card-header>
                    <mat-card-subtitle>Line {{ i + 1 }}</mat-card-subtitle>
                    <button mat-icon-button color="warn" (click)="removeLine(i)"
                            [disabled]="linesArray.length <= 1" matTooltip="Remove line">
                      <mat-icon>close</mat-icon>
                    </button>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="form-row">
                      <mat-form-field appearance="outline">
                        <mat-label>CPT Code</mat-label>
                        <input matInput formControlName="cptCode" placeholder="e.g. 99213">
                        <mat-error *ngIf="line.get('cptCode')?.hasError('required')">Required</mat-error>
                      </mat-form-field>

                      <mat-form-field appearance="outline" class="narrow-field">
                        <mat-label>Units</mat-label>
                        <input matInput type="number" formControlName="units" min="1">
                        <mat-error *ngIf="line.get('units')?.hasError('required')">Required</mat-error>
                      </mat-form-field>

                      <mat-form-field appearance="outline">
                        <mat-label>Billed Amount</mat-label>
                        <input matInput type="number" formControlName="billedAmount" min="0" step="0.01">
                        <mat-error *ngIf="line.get('billedAmount')?.hasError('required')">Required</mat-error>
                      </mat-form-field>
                    </div>

                    <!-- Line-level diagnoses -->
                    <div formArrayName="diagnosisCodes" class="line-diagnoses">
                      <span class="line-dx-label">Line Diagnoses:</span>
                      <div *ngFor="let dxCtrl of getLineDiagnoses(i).controls; let j = index" class="inline-row">
                        <mat-form-field appearance="outline" class="narrow-field">
                          <mat-label>Dx Code</mat-label>
                          <input matInput [formControlName]="j" placeholder="e.g. J06.9">
                        </mat-form-field>
                        <button mat-icon-button color="warn" (click)="removeLineDiagnosis(i, j)" matTooltip="Remove">
                          <mat-icon>close</mat-icon>
                        </button>
                      </div>
                      <button mat-stroked-button type="button" (click)="addLineDiagnosis(i)">
                        <mat-icon>add</mat-icon> Add Dx
                      </button>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>

              <button mat-stroked-button color="primary" (click)="addLine()">
                <mat-icon>add</mat-icon> Add Line
              </button>
            </div>

            <div class="step-actions">
              <button mat-stroked-button matStepperPrevious>Back</button>
              <button mat-raised-button color="primary" matStepperNext
                      [disabled]="linesArray.length === 0 || linesForm.invalid">Next</button>
            </div>
          </form>
        </mat-step>

        <!-- Step 4: Review & Submit -->
        <mat-step label="Review & Submit">
          <div class="step-content">
            <h3>Review Claim Submission</h3>

            <mat-card class="review-section">
              <mat-card-subtitle>Claim Header</mat-card-subtitle>
              <mat-card-content>
                <div class="review-grid">
                  <div><strong>Member ID:</strong> {{ headerForm.value.memberId }}</div>
                  <div><strong>Provider ID:</strong> {{ headerForm.value.providerId }}</div>
                  <div><strong>Service From:</strong> {{ headerForm.value.serviceFrom | date:'mediumDate' }}</div>
                  <div><strong>Service To:</strong> {{ headerForm.value.serviceTo | date:'mediumDate' }}</div>
                  <div><strong>Received Date:</strong> {{ headerForm.value.receivedDate | date:'mediumDate' }}</div>
                </div>
              </mat-card-content>
            </mat-card>

            <mat-card class="review-section">
              <mat-card-subtitle>Diagnoses ({{ diagnosesArray.length }})</mat-card-subtitle>
              <mat-card-content>
                <table mat-table [dataSource]="diagnosesArray.value" class="full-width">
                  <ng-container matColumnDef="codeSystem">
                    <th mat-header-cell *matHeaderCellDef>System</th>
                    <td mat-cell *matCellDef="let dx">{{ dx.codeSystem }}</td>
                  </ng-container>
                  <ng-container matColumnDef="code">
                    <th mat-header-cell *matHeaderCellDef>Code</th>
                    <td mat-cell *matCellDef="let dx">{{ dx.code }}</td>
                  </ng-container>
                  <ng-container matColumnDef="isPrimary">
                    <th mat-header-cell *matHeaderCellDef>Primary</th>
                    <td mat-cell *matCellDef="let dx">{{ dx.isPrimary ? 'Yes' : 'No' }}</td>
                  </ng-container>
                  <tr mat-header-row *matHeaderRowDef="['codeSystem', 'code', 'isPrimary']"></tr>
                  <tr mat-row *matRowDef="let row; columns: ['codeSystem', 'code', 'isPrimary'];"></tr>
                </table>
              </mat-card-content>
            </mat-card>

            <mat-card class="review-section">
              <mat-card-subtitle>Claim Lines ({{ linesArray.length }})</mat-card-subtitle>
              <mat-card-content>
                <table mat-table [dataSource]="linesArray.value" class="full-width">
                  <ng-container matColumnDef="lineNumber">
                    <th mat-header-cell *matHeaderCellDef>Line #</th>
                    <td mat-cell *matCellDef="let line; let i = index">{{ i + 1 }}</td>
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
                    <td mat-cell *matCellDef="let line">{{ line.billedAmount | number:'1.2-2' }}</td>
                  </ng-container>
                  <tr mat-header-row *matHeaderRowDef="['lineNumber', 'cptCode', 'units', 'billedAmount']"></tr>
                  <tr mat-row *matRowDef="let row; columns: ['lineNumber', 'cptCode', 'units', 'billedAmount'];"></tr>
                </table>
              </mat-card-content>
            </mat-card>

            <p *ngIf="submitError" class="submit-error">{{ submitError }}</p>
          </div>

          <div class="step-actions">
            <button mat-stroked-button matStepperPrevious>Back</button>
            <button mat-raised-button color="primary"
                    [disabled]="submitting"
                    (click)="submitClaim()">
              <mat-icon>send</mat-icon>
              {{ submitting ? 'Submitting...' : 'Submit Claim' }}
            </button>
          </div>
        </mat-step>
      </mat-stepper>
    </div>
  `,
  styles: [`
    .claim-submit-container {
      padding: 24px;
      max-width: 960px;
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
    .step-content {
      padding: 16px 0;
    }
    .step-hint {
      color: rgba(0, 0, 0, 0.54);
      font-size: 14px;
      margin-bottom: 16px;
    }
    .step-actions {
      display: flex;
      gap: 12px;
      padding-top: 16px;
    }
    .form-row {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
    }
    .form-row mat-form-field {
      flex: 1;
      min-width: 200px;
    }
    .narrow-field {
      max-width: 160px;
    }
    .inline-row {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 8px;
    }
    .line-card {
      margin-bottom: 16px;
    }
    .line-card mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .line-diagnoses {
      margin-top: 8px;
      padding-top: 8px;
      border-top: 1px dashed rgba(0, 0, 0, 0.12);
    }
    .line-dx-label {
      font-size: 13px;
      color: rgba(0, 0, 0, 0.54);
      display: block;
      margin-bottom: 8px;
    }
    .review-section {
      margin-bottom: 16px;
    }
    .review-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 12px;
      padding: 8px 0;
    }
    .full-width {
      width: 100%;
    }
    .result-card {
      margin-bottom: 24px;
    }
    .result-card mat-card-title {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .result-grid {
      display: flex;
      gap: 32px;
      flex-wrap: wrap;
      margin: 16px 0;
    }
    .result-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .result-item .label {
      font-size: 12px;
      color: rgba(0, 0, 0, 0.54);
    }
    .result-item .value {
      font-size: 16px;
      font-weight: 500;
    }
    .result-actions {
      display: flex;
      gap: 12px;
    }
    .submit-error {
      color: #c62828;
      font-weight: 500;
      margin-top: 12px;
    }
  `],
})
export class ClaimSubmitComponent implements OnDestroy {
  private destroy$ = new Subject<void>();

  submitting = false;
  submitError: string | null = null;
  submitResult: any = null;

  headerForm: FormGroup;
  diagnosesForm: FormGroup;
  linesForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private apollo: Apollo,
    private snackBar: MatSnackBar,
  ) {
    this.headerForm = this.fb.group({
      memberId: ['', Validators.required],
      providerId: ['', Validators.required],
      serviceFrom: [null, Validators.required],
      serviceTo: [null, Validators.required],
      receivedDate: [null, Validators.required],
    });

    this.diagnosesForm = this.fb.group({
      diagnoses: this.fb.array([this.createDiagnosisGroup()]),
    });

    this.linesForm = this.fb.group({
      lines: this.fb.array([this.createLineGroup()]),
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // --- Diagnosis helpers ---

  get diagnosesArray(): FormArray {
    return this.diagnosesForm.get('diagnoses') as FormArray;
  }

  createDiagnosisGroup(): FormGroup {
    return this.fb.group({
      codeSystem: ['ICD-10', Validators.required],
      code: ['', Validators.required],
      isPrimary: [false],
    });
  }

  addDiagnosis(): void {
    this.diagnosesArray.push(this.createDiagnosisGroup());
  }

  removeDiagnosis(index: number): void {
    this.diagnosesArray.removeAt(index);
  }

  // --- Line helpers ---

  get linesArray(): FormArray {
    return this.linesForm.get('lines') as FormArray;
  }

  createLineGroup(): FormGroup {
    return this.fb.group({
      cptCode: ['', Validators.required],
      units: [1, [Validators.required, Validators.min(1)]],
      billedAmount: [0, [Validators.required, Validators.min(0)]],
      diagnosisCodes: this.fb.array([]),
    });
  }

  addLine(): void {
    this.linesArray.push(this.createLineGroup());
  }

  removeLine(index: number): void {
    this.linesArray.removeAt(index);
  }

  getLineDiagnoses(lineIndex: number): FormArray {
    return this.linesArray.at(lineIndex).get('diagnosisCodes') as FormArray;
  }

  addLineDiagnosis(lineIndex: number): void {
    this.getLineDiagnoses(lineIndex).push(this.fb.control(''));
  }

  removeLineDiagnosis(lineIndex: number, dxIndex: number): void {
    this.getLineDiagnoses(lineIndex).removeAt(dxIndex);
  }

  // --- Submit ---

  submitClaim(): void {
    this.submitting = true;
    this.submitError = null;

    const header = this.headerForm.value;
    const diagnoses = this.diagnosesArray.value.map((dx: any) => ({
      codeSystem: dx.codeSystem,
      code: dx.code,
      isPrimary: dx.isPrimary,
    }));
    const lines = this.linesArray.value.map((line: any, i: number) => ({
      lineNumber: i + 1,
      cptCode: line.cptCode,
      units: line.units,
      billedAmount: line.billedAmount,
      diagnosisCodes: line.diagnosisCodes?.filter((c: string) => c) || [],
    }));

    const idempotencyKey = crypto.randomUUID();

    const input = {
      idempotencyKey,
      memberId: header.memberId,
      providerId: header.providerId,
      serviceFromDate: this.formatDate(header.serviceFrom),
      serviceToDate: this.formatDate(header.serviceTo),
      receivedDate: this.formatDate(header.receivedDate),
      diagnoses,
      lines,
    };

    this.apollo
      .mutate<any>({
        mutation: SUBMIT_CLAIM,
        variables: { input },
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ data }) => {
          this.submitting = false;
          this.submitResult = data.submitClaim;
          if (this.submitResult.alreadyExisted) {
            this.snackBar.open('This claim already existed (duplicate idempotency key).', 'OK', { duration: 5000 });
          } else {
            this.snackBar.open('Claim submitted successfully!', 'OK', { duration: 3000 });
          }
        },
        error: (err) => {
          this.submitting = false;
          this.submitError = err.message || 'Failed to submit claim.';
        },
      });
  }

  resetForm(): void {
    this.submitResult = null;
    this.submitError = null;
    this.headerForm.reset();
    this.diagnosesForm.setControl('diagnoses', this.fb.array([this.createDiagnosisGroup()]));
    this.linesForm.setControl('lines', this.fb.array([this.createLineGroup()]));
  }

  private formatDate(date: Date | null): string | null {
    if (!date) return null;
    return date.toISOString().split('T')[0];
  }
}
