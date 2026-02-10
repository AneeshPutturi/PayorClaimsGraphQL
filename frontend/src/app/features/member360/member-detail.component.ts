import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Apollo, gql } from 'apollo-angular';
import { Subject } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';

const MEMBER_360_QUERY = gql`
  query Member360($memberId: ID!, $asOf: String!) {
    memberById(id: $memberId) {
      id
      firstName
      lastName
      status
      dob
      externalMemberNumber
      ssnMasked
      emailMasked
      phoneMasked
      activeCoverage(asOf: $asOf) {
        id
        coverageStatus
        startDate
        endDate
        plan {
          planCode
          name
          effectiveBenefits(asOf: $asOf) {
            category
            network
            copayAmount
            coinsurancePercent
            deductibleApplies
          }
        }
      }
      recentClaims(limit: 5) {
        id
        claimNumber
        status
        totalBilled
        totalAllowed
        totalPaid
        receivedDate
        provider {
          npi
          name
        }
        lines {
          lineNumber
          cptCode
          billedAmount
          lineStatus
        }
        diagnoses {
          codeSystem
          code
          isPrimary
          lineNumber
        }
      }
    }
  }
`;

interface Benefit {
  category: string;
  network: string;
  copayAmount: number;
  coinsurancePercent: number;
  deductibleApplies: boolean;
}

interface Plan {
  planCode: string;
  name: string;
  effectiveBenefits: Benefit[];
}

interface Coverage {
  id: string;
  coverageStatus: string;
  startDate: string;
  endDate: string;
  plan: Plan;
}

interface ClaimLine {
  lineNumber: number;
  cptCode: string;
  billedAmount: number;
  lineStatus: string;
}

interface Diagnosis {
  codeSystem: string;
  code: string;
  isPrimary: boolean;
  lineNumber: number;
}

interface Claim {
  id: string;
  claimNumber: string;
  status: string;
  totalBilled: number;
  totalAllowed: number;
  totalPaid: number;
  receivedDate: string;
  provider: { npi: string; name: string };
  lines: ClaimLine[];
  diagnoses: Diagnosis[];
}

interface Member {
  id: string;
  firstName: string;
  lastName: string;
  status: string;
  dob: string;
  externalMemberNumber: string;
  ssnMasked: string;
  emailMasked: string;
  phoneMasked: string;
  activeCoverage: Coverage[];
  recentClaims: Claim[];
}

interface Member360Result {
  memberById: Member;
}

@Component({
  selector: 'app-member-detail',
  standalone: false,
  template: `
    <div class="member-detail-container">
      <button mat-button (click)="goBack()" class="back-button">
        <mat-icon>arrow_back</mat-icon>
        Back to Members
      </button>

      <!-- Loading -->
      @if (loading) {
        <app-loading message="Loading member details…"></app-loading>
      }

      <!-- Error -->
      @if (error) {
        <mat-card class="error-card">
          <mat-card-content>
            <div class="error-content">
              <mat-icon color="warn">error_outline</mat-icon>
              <span>{{ error }}</span>
              <button mat-button color="primary" (click)="reload()">Retry</button>
            </div>
          </mat-card-content>
        </mat-card>
      }

      @if (member) {
        <!-- Member Summary Card -->
        <mat-card class="section-card summary-card">
          <mat-card-header>
            <mat-icon mat-card-avatar class="avatar-icon">person</mat-icon>
            <mat-card-title>{{ member.firstName }} {{ member.lastName }}</mat-card-title>
            <mat-card-subtitle>Member # {{ member.externalMemberNumber }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content class="summary-content">
            <div class="summary-grid">
              <div class="summary-item">
                <span class="label">Status</span>
                <app-status-badge [status]="member.status"></app-status-badge>
              </div>
              <div class="summary-item">
                <span class="label">Date of Birth</span>
                <span class="value">{{ member.dob | date:'MM/dd/yyyy' }}</span>
              </div>
              <div class="summary-item">
                <span class="label">SSN</span>
                <span class="value">{{ member.ssnMasked }}</span>
              </div>
              <div class="summary-item">
                <span class="label">Email</span>
                <span class="value">{{ member.emailMasked }}</span>
              </div>
              <div class="summary-item">
                <span class="label">Phone</span>
                <span class="value">{{ member.phoneMasked }}</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Coverage & Plan Cards -->
        @for (cov of member.activeCoverage; track cov.id) {
          <mat-card class="section-card">
            <mat-card-header>
              <mat-icon mat-card-avatar class="avatar-icon">verified_user</mat-icon>
              <mat-card-title>{{ cov.plan.name }}</mat-card-title>
              <mat-card-subtitle>Plan Code: {{ cov.plan.planCode }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="coverage-grid">
                <div class="summary-item">
                  <span class="label">Coverage Status</span>
                  <app-status-badge [status]="cov.coverageStatus"></app-status-badge>
                </div>
                <div class="summary-item">
                  <span class="label">Start Date</span>
                  <span class="value">{{ cov.startDate | date:'MM/dd/yyyy' }}</span>
                </div>
                <div class="summary-item">
                  <span class="label">End Date</span>
                  <span class="value">{{ cov.endDate | date:'MM/dd/yyyy' }}</span>
                </div>
              </div>

              <!-- Benefits Table -->
              @if (cov.plan.effectiveBenefits && cov.plan.effectiveBenefits.length > 0) {
                <h3 class="sub-heading">Benefits</h3>
                <table mat-table [dataSource]="cov.plan.effectiveBenefits" class="benefits-table">
                  <ng-container matColumnDef="category">
                    <th mat-header-cell *matHeaderCellDef>Category</th>
                    <td mat-cell *matCellDef="let b">{{ b.category }}</td>
                  </ng-container>

                  <ng-container matColumnDef="network">
                    <th mat-header-cell *matHeaderCellDef>Network</th>
                    <td mat-cell *matCellDef="let b">{{ b.network }}</td>
                  </ng-container>

                  <ng-container matColumnDef="copayAmount">
                    <th mat-header-cell *matHeaderCellDef>Copay</th>
                    <td mat-cell *matCellDef="let b">{{ b.copayAmount | money }}</td>
                  </ng-container>

                  <ng-container matColumnDef="coinsurancePercent">
                    <th mat-header-cell *matHeaderCellDef>Coinsurance</th>
                    <td mat-cell *matCellDef="let b">{{ b.coinsurancePercent }}%</td>
                  </ng-container>

                  <ng-container matColumnDef="deductibleApplies">
                    <th mat-header-cell *matHeaderCellDef>Deductible</th>
                    <td mat-cell *matCellDef="let b">
                      <mat-icon [class.yes]="b.deductibleApplies" [class.no]="!b.deductibleApplies">
                        {{ b.deductibleApplies ? 'check_circle' : 'cancel' }}
                      </mat-icon>
                    </td>
                  </ng-container>

                  <tr mat-header-row *matHeaderRowDef="benefitColumns"></tr>
                  <tr mat-row *matRowDef="let row; columns: benefitColumns;"></tr>
                </table>
              }
            </mat-card-content>
          </mat-card>
        }

        @if (member.activeCoverage.length === 0) {
          <mat-card class="section-card">
            <mat-card-content>
              <app-empty-state
                icon="shield"
                title="No active coverage"
                message="This member does not have active coverage as of today."
              ></app-empty-state>
            </mat-card-content>
          </mat-card>
        }

        <!-- Recent Claims Card -->
        <mat-card class="section-card">
          <mat-card-header>
            <mat-icon mat-card-avatar class="avatar-icon">description</mat-icon>
            <mat-card-title>Recent Claims</mat-card-title>
            <mat-card-subtitle>Last 5 claims</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            @if (member.recentClaims.length === 0) {
              <app-empty-state
                icon="receipt_long"
                title="No recent claims"
                message="This member has no claims on file."
              ></app-empty-state>
            } @else {
              <mat-accordion multi>
                @for (claim of member.recentClaims; track claim.id) {
                  <mat-expansion-panel>
                    <mat-expansion-panel-header>
                      <mat-panel-title class="claim-panel-title">
                        <span class="claim-number">{{ claim.claimNumber }}</span>
                        <app-status-badge [status]="claim.status"></app-status-badge>
                      </mat-panel-title>
                      <mat-panel-description class="claim-panel-desc">
                        <span>{{ claim.totalBilled | money }}</span>
                        <span class="claim-date">{{ claim.receivedDate | date:'MM/dd/yyyy' }}</span>
                        <span class="claim-provider">{{ claim.provider.name }}</span>
                      </mat-panel-description>
                    </mat-expansion-panel-header>

                    <div class="claim-detail-content">
                      <div class="claim-summary-row">
                        <div class="claim-stat">
                          <span class="label">Total Billed</span>
                          <span class="value">{{ claim.totalBilled | money }}</span>
                        </div>
                        <div class="claim-stat">
                          <span class="label">Total Allowed</span>
                          <span class="value">{{ claim.totalAllowed | money }}</span>
                        </div>
                        <div class="claim-stat">
                          <span class="label">Total Paid</span>
                          <span class="value">{{ claim.totalPaid | money }}</span>
                        </div>
                        <div class="claim-stat">
                          <span class="label">Provider NPI</span>
                          <span class="value">{{ claim.provider.npi }}</span>
                        </div>
                      </div>

                      <!-- Claim Lines -->
                      @if (claim.lines && claim.lines.length > 0) {
                        <h4 class="sub-heading">Service Lines</h4>
                        <table mat-table [dataSource]="claim.lines" class="inner-table">
                          <ng-container matColumnDef="lineNumber">
                            <th mat-header-cell *matHeaderCellDef>Line #</th>
                            <td mat-cell *matCellDef="let l">{{ l.lineNumber }}</td>
                          </ng-container>
                          <ng-container matColumnDef="cptCode">
                            <th mat-header-cell *matHeaderCellDef>CPT Code</th>
                            <td mat-cell *matCellDef="let l">{{ l.cptCode }}</td>
                          </ng-container>
                          <ng-container matColumnDef="billedAmount">
                            <th mat-header-cell *matHeaderCellDef>Billed</th>
                            <td mat-cell *matCellDef="let l">{{ l.billedAmount | money }}</td>
                          </ng-container>
                          <ng-container matColumnDef="lineStatus">
                            <th mat-header-cell *matHeaderCellDef>Status</th>
                            <td mat-cell *matCellDef="let l">
                              <app-status-badge [status]="l.lineStatus"></app-status-badge>
                            </td>
                          </ng-container>
                          <tr mat-header-row *matHeaderRowDef="lineColumns"></tr>
                          <tr mat-row *matRowDef="let row; columns: lineColumns;"></tr>
                        </table>
                      }

                      <!-- Diagnoses -->
                      @if (claim.diagnoses && claim.diagnoses.length > 0) {
                        <h4 class="sub-heading">Diagnoses</h4>
                        <table mat-table [dataSource]="claim.diagnoses" class="inner-table">
                          <ng-container matColumnDef="code">
                            <th mat-header-cell *matHeaderCellDef>Code</th>
                            <td mat-cell *matCellDef="let d">{{ d.code }}</td>
                          </ng-container>
                          <ng-container matColumnDef="codeSystem">
                            <th mat-header-cell *matHeaderCellDef>System</th>
                            <td mat-cell *matCellDef="let d">{{ d.codeSystem }}</td>
                          </ng-container>
                          <ng-container matColumnDef="isPrimary">
                            <th mat-header-cell *matHeaderCellDef>Primary</th>
                            <td mat-cell *matCellDef="let d">
                              <mat-icon [class.yes]="d.isPrimary" [class.no]="!d.isPrimary">
                                {{ d.isPrimary ? 'check_circle' : 'remove_circle_outline' }}
                              </mat-icon>
                            </td>
                          </ng-container>
                          <ng-container matColumnDef="diagLineNumber">
                            <th mat-header-cell *matHeaderCellDef>Line #</th>
                            <td mat-cell *matCellDef="let d">{{ d.lineNumber }}</td>
                          </ng-container>
                          <tr mat-header-row *matHeaderRowDef="diagColumns"></tr>
                          <tr mat-row *matRowDef="let row; columns: diagColumns;"></tr>
                        </table>
                      }

                      <div class="claim-actions">
                        <button mat-stroked-button color="primary" (click)="goToClaim(claim.id)">
                          <mat-icon>open_in_new</mat-icon>
                          View Full Claim
                        </button>
                      </div>
                    </div>
                  </mat-expansion-panel>
                }
              </mat-accordion>
            }
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .member-detail-container {
      max-width: 1200px;
      margin: 0 auto;
    }

    .back-button {
      margin-bottom: 16px;
    }

    .section-card {
      margin-bottom: 24px;
    }

    .avatar-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: rgba(0, 0, 0, 0.54);
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .error-card {
      margin-bottom: 24px;
    }

    .error-content {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .summary-content {
      padding-top: 16px;
    }

    .summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 20px;
    }

    .coverage-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 20px;
      margin-bottom: 16px;
    }

    .summary-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .label {
      font-size: 12px;
      font-weight: 500;
      color: rgba(0, 0, 0, 0.54);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .value {
      font-size: 14px;
      color: rgba(0, 0, 0, 0.87);
    }

    .sub-heading {
      font-size: 14px;
      font-weight: 500;
      color: rgba(0, 0, 0, 0.87);
      margin: 24px 0 12px 0;
    }

    .benefits-table,
    .inner-table {
      width: 100%;
    }

    .yes {
      color: #2e7d32;
    }

    .no {
      color: #bdbdbd;
    }

    .claim-panel-title {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .claim-number {
      font-weight: 500;
    }

    .claim-panel-desc {
      display: flex;
      align-items: center;
      gap: 16px;
      justify-content: flex-end;
    }

    .claim-date {
      color: rgba(0, 0, 0, 0.54);
    }

    .claim-provider {
      color: rgba(0, 0, 0, 0.54);
      font-style: italic;
    }

    .claim-detail-content {
      padding: 8px 0;
    }

    .claim-summary-row {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
      gap: 16px;
      margin-bottom: 8px;
    }

    .claim-stat {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .claim-actions {
      margin-top: 16px;
      display: flex;
      justify-content: flex-end;
    }
  `],
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  member: Member | null = null;
  loading = false;
  error: string | null = null;

  benefitColumns = ['category', 'network', 'copayAmount', 'coinsurancePercent', 'deductibleApplies'];
  lineColumns = ['lineNumber', 'cptCode', 'billedAmount', 'lineStatus'];
  diagColumns = ['code', 'codeSystem', 'isPrimary', 'diagLineNumber'];

  private memberId: string = '';
  private destroy$ = new Subject<void>();

  constructor(
    private apollo: Apollo,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        this.memberId = params.get('id') || '';
        if (this.memberId) {
          this.fetchMember();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchMember(): void {
    this.loading = true;
    this.error = null;

    const today = new Date();
    const asOf = today.toISOString().slice(0, 10); // YYYY-MM-DD

    this.apollo
      .query<Member360Result>({
        query: MEMBER_360_QUERY,
        variables: {
          memberId: this.memberId,
          asOf,
        },
        fetchPolicy: 'network-only',
      })
      .subscribe({
        next: ({ data }) => {
          this.member = data?.memberById ?? null;
          this.loading = false;
        },
        error: (err) => {
          this.error = err?.message || 'Failed to load member details.';
          this.loading = false;
        },
      });
  }

  reload(): void {
    this.fetchMember();
  }

  goBack(): void {
    this.router.navigate(['/members']);
  }

  goToClaim(claimId: string): void {
    this.router.navigate(['/claims', claimId]);
  }
}
