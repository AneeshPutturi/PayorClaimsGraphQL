import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
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
  selector: 'app-work-queue',
  standalone: false,
  template: `
    <div class="work-queue-container">
      <div class="page-header">
        <h2>Adjudication Work Queue</h2>
      </div>

      <!-- Filter -->
      <mat-card class="filter-card">
        <mat-card-content>
          <div class="filter-bar">
            <mat-form-field appearance="outline">
              <mat-label>Status Filter</mat-label>
              <mat-select [formControl]="statusFilter" (selectionChange)="onFilterChange()">
                <mat-option value="Received">Received</mat-option>
                <mat-option value="Pending">Pending</mat-option>
                <mat-option value="">All Statuses</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
        </mat-card-content>
      </mat-card>

      <!-- Loading -->
      <app-loading *ngIf="loading" message="Loading work queue..."></app-loading>

      <!-- Error -->
      <mat-card *ngIf="error" class="error-card">
        <mat-card-content>
          <mat-icon color="warn">error</mat-icon>
          <span>{{ error }}</span>
          <button mat-button color="primary" (click)="fetchClaims()">Retry</button>
        </mat-card-content>
      </mat-card>

      <!-- Empty -->
      <app-empty-state
        *ngIf="!loading && !error && claims.length === 0"
        icon="assignment"
        title="No claims in queue"
        message="There are no claims matching the selected status filter.">
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
    .work-queue-container {
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
    }
    .filter-bar mat-form-field {
      width: 200px;
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
export class WorkQueueComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  displayedColumns = ['claimNumber', 'status', 'totalBilled', 'receivedDate'];
  claims: any[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  loading = false;
  error: string | null = null;

  statusFilter = new FormControl('Received');

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
    const status = this.statusFilter.value;
    if (status) {
      filter.status = status;
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
          this.error = err.message || 'Failed to load work queue.';
        },
      });
  }

  onFilterChange(): void {
    this.pageIndex = 0;
    this.fetchClaims();
  }

  onPage(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.pageIndex = event.pageIndex;
    this.fetchClaims();
  }

  onRowClick(row: any): void {
    this.router.navigate(['/adjudication', row.id]);
  }
}
