import { Component, OnInit, OnDestroy, ViewChild, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl, FormGroup } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { Apollo, gql } from 'apollo-angular';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

const CLAIMS_LIST = gql`
  query ClaimsList($filter: ClaimFilterInput, $page: PageInput, $sortField: ClaimSortField, $sortDir: SortDirection) {
    claims(filter: $filter, page: $page, sortField: $sortField, sortDir: $sortDir) {
      totalCount
      skip
      take
      items {
        id
        claimNumber
        status
        totalBilled
        receivedDate
        memberId
      }
    }
  }
`;

@Component({
  selector: 'app-claim-list',
  standalone: false,
  template: `
    <div class="claim-list-container">
      <div class="page-header">
        <h2>Claims</h2>
        <button mat-raised-button color="primary" routerLink="submit">
          <mat-icon>add</mat-icon> Submit Claim
        </button>
      </div>

      <!-- Filter Bar -->
      <mat-card class="filter-card">
        <mat-card-content>
          <form [formGroup]="filterForm" class="filter-bar">
            <mat-form-field appearance="outline">
              <mat-label>Status</mat-label>
              <mat-select formControlName="status">
                <mat-option value="">All</mat-option>
                <mat-option value="Received">Received</mat-option>
                <mat-option value="Pending">Pending</mat-option>
                <mat-option value="Paid">Paid</mat-option>
                <mat-option value="Denied">Denied</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Date From</mat-label>
              <input matInput [matDatepicker]="dateFrom" formControlName="dateFrom">
              <mat-datepicker-toggle matIconSuffix [for]="dateFrom"></mat-datepicker-toggle>
              <mat-datepicker #dateFrom></mat-datepicker>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Date To</mat-label>
              <input matInput [matDatepicker]="dateTo" formControlName="dateTo">
              <mat-datepicker-toggle matIconSuffix [for]="dateTo"></mat-datepicker-toggle>
              <mat-datepicker #dateTo></mat-datepicker>
            </mat-form-field>

            <button mat-stroked-button color="primary" (click)="applyFilter()">
              <mat-icon>search</mat-icon> Search
            </button>
            <button mat-stroked-button (click)="clearFilter()">Clear</button>
          </form>
        </mat-card-content>
      </mat-card>

      <!-- Loading -->
      <app-loading *ngIf="loading" message="Loading claims..."></app-loading>

      <!-- Error -->
      <mat-card *ngIf="error" class="error-card">
        <mat-card-content>
          <mat-icon color="warn">error</mat-icon>
          <span>{{ error }}</span>
          <button mat-button color="primary" (click)="fetchClaims()">Retry</button>
        </mat-card-content>
      </mat-card>

      <!-- Empty state -->
      <app-empty-state
        *ngIf="!loading && !error && claims.length === 0"
        icon="description"
        title="No claims found"
        message="Try adjusting your filters or submit a new claim.">
      </app-empty-state>

      <!-- Table -->
      <div class="table-container" *ngIf="!loading && !error && claims.length > 0">
        <table mat-table [dataSource]="claims" class="full-width">
          <ng-container matColumnDef="claimNumber">
            <th mat-header-cell *matHeaderCellDef>Claim #</th>
            <td mat-cell *matCellDef="let row">{{ row.claimNumber }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let row">
              <app-status-badge [status]="row.status"></app-status-badge>
            </td>
          </ng-container>

          <ng-container matColumnDef="totalBilled">
            <th mat-header-cell *matHeaderCellDef>Total Billed</th>
            <td mat-cell *matCellDef="let row">{{ row.totalBilled | money }}</td>
          </ng-container>

          <ng-container matColumnDef="receivedDate">
            <th mat-header-cell *matHeaderCellDef>Received Date</th>
            <td mat-cell *matCellDef="let row">{{ row.receivedDate | date:'mediumDate' }}</td>
          </ng-container>

          <ng-container matColumnDef="memberId">
            <th mat-header-cell *matHeaderCellDef>Member ID</th>
            <td mat-cell *matCellDef="let row">{{ row.memberId }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"
              class="clickable-row"
              (click)="onRowClick(row)"></tr>
        </table>

        <mat-paginator
          [length]="totalCount"
          [pageSize]="pageSize"
          [pageIndex]="pageIndex"
          [pageSizeOptions]="[10, 25, 50]"
          (page)="onPage($event)"
          showFirstLastButtons>
        </mat-paginator>
      </div>
    </div>
  `,
  styles: [`
    .claim-list-container {
      padding: 24px;
    }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }
    .page-header h2 {
      margin: 0;
      font-size: 24px;
      font-weight: 500;
    }
    .filter-card {
      margin-bottom: 16px;
    }
    .filter-bar {
      display: flex;
      gap: 16px;
      align-items: center;
      flex-wrap: wrap;
    }
    .filter-bar mat-form-field {
      width: 180px;
    }
    .table-container {
      background: white;
      border-radius: 8px;
      overflow: hidden;
    }
    .full-width {
      width: 100%;
    }
    .clickable-row {
      cursor: pointer;
    }
    .clickable-row:hover {
      background-color: rgba(0, 0, 0, 0.04);
    }
    .error-card mat-card-content {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #c62828;
    }
  `],
})
export class ClaimListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  displayedColumns = ['claimNumber', 'status', 'totalBilled', 'receivedDate', 'memberId'];
  claims: any[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  loading = false;
  error: string | null = null;

  filterForm = new FormGroup({
    status: new FormControl(''),
    dateFrom: new FormControl<Date | null>(null),
    dateTo: new FormControl<Date | null>(null),
  });

  constructor(
    private apollo: Apollo,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.fetchClaims();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchClaims(): void {
    this.loading = true;
    this.error = null;

    const filter: any = {};
    const { status, dateFrom, dateTo } = this.filterForm.value;
    if (status) {
      filter.status = status;
    }
    if (dateFrom) {
      filter.receivedFrom = dateFrom.toISOString().split('T')[0];
    }
    if (dateTo) {
      filter.receivedTo = dateTo.toISOString().split('T')[0];
    }

    this.apollo
      .watchQuery<any>({
        query: CLAIMS_LIST,
        variables: {
          filter: Object.keys(filter).length > 0 ? filter : null,
          page: { skip: this.pageIndex * this.pageSize, take: this.pageSize },
        },
        fetchPolicy: 'network-only',
      })
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ data, loading }) => {
          this.loading = loading;
          if (data?.claims) {
            this.claims = data.claims.items;
            this.totalCount = data.claims.totalCount;
          }
        },
        error: (err) => {
          this.loading = false;
          this.error = err.message || 'Failed to load claims.';
        },
      });
  }

  applyFilter(): void {
    this.pageIndex = 0;
    this.fetchClaims();
  }

  clearFilter(): void {
    this.filterForm.reset({ status: '', dateFrom: null, dateTo: null });
    this.pageIndex = 0;
    this.fetchClaims();
  }

  onPage(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.pageIndex = event.pageIndex;
    this.fetchClaims();
  }

  onRowClick(row: any): void {
    this.router.navigate(['/claims', row.id]);
  }
}
