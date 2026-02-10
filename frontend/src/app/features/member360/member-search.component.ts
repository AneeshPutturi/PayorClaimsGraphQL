import { Component, OnInit, OnDestroy, ViewChild, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { Apollo, gql } from 'apollo-angular';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';

const MEMBERS_LIST_QUERY = gql`
  query MembersList(
    $filter: MemberFilterInput
    $page: PageInput
    $sortField: MemberSortField
    $sortDir: SortDirection
  ) {
    members(filter: $filter, page: $page, sortField: $sortField, sortDir: $sortDir) {
      totalCount
      skip
      take
      items {
        id
        firstName
        lastName
        status
        dob
        externalMemberNumber
      }
    }
  }
`;

interface MemberRow {
  id: string;
  firstName: string;
  lastName: string;
  status: string;
  dob: string;
  externalMemberNumber: string;
}

interface MembersListResult {
  members: {
    totalCount: number;
    skip: number;
    take: number;
    items: MemberRow[];
  };
}

@Component({
  selector: 'app-member-search',
  standalone: false,
  template: `
    <div class="member-search-container">
      <h2 class="page-title">Members</h2>

      <mat-card class="search-card">
        <mat-card-content>
          <mat-form-field appearance="outline" class="search-field">
            <mat-label>Search by name</mat-label>
            <input matInput [formControl]="searchControl" placeholder="Enter first or last name…" />
            <mat-icon matPrefix>search</mat-icon>
            @if (searchControl.value) {
              <button matSuffix mat-icon-button (click)="searchControl.setValue('')" aria-label="Clear">
                <mat-icon>close</mat-icon>
              </button>
            }
          </mat-form-field>
        </mat-card-content>
      </mat-card>

      <!-- Loading state -->
      @if (loading) {
        <app-loading message="Loading members…"></app-loading>
      }

      <!-- Error state -->
      @if (error) {
        <mat-card class="error-card">
          <mat-card-content>
            <div class="error-content">
              <mat-icon color="warn">error_outline</mat-icon>
              <span>{{ error }}</span>
              <button mat-button color="primary" (click)="fetchMembers()">Retry</button>
            </div>
          </mat-card-content>
        </mat-card>
      }

      <!-- Empty state -->
      @if (!loading && !error && members.length === 0) {
        <app-empty-state
          icon="people_outline"
          title="No members found"
          [message]="searchControl.value ? 'Try a different search term.' : 'No members available.'"
        ></app-empty-state>
      }

      <!-- Results table -->
      @if (!loading && !error && members.length > 0) {
        <mat-card class="table-card">
          <mat-card-content class="table-content">
            <table mat-table [dataSource]="members" class="members-table">

              <ng-container matColumnDef="externalMemberNumber">
                <th mat-header-cell *matHeaderCellDef>Member #</th>
                <td mat-cell *matCellDef="let row">{{ row.externalMemberNumber }}</td>
              </ng-container>

              <ng-container matColumnDef="firstName">
                <th mat-header-cell *matHeaderCellDef>First Name</th>
                <td mat-cell *matCellDef="let row">{{ row.firstName }}</td>
              </ng-container>

              <ng-container matColumnDef="lastName">
                <th mat-header-cell *matHeaderCellDef>Last Name</th>
                <td mat-cell *matCellDef="let row">{{ row.lastName }}</td>
              </ng-container>

              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let row">
                  <app-status-badge [status]="row.status"></app-status-badge>
                </td>
              </ng-container>

              <ng-container matColumnDef="dob">
                <th mat-header-cell *matHeaderCellDef>Date of Birth</th>
                <td mat-cell *matCellDef="let row">{{ row.dob | date:'MM/dd/yyyy' }}</td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"
                  class="clickable-row"
                  (click)="goToMember(row.id)"></tr>
            </table>
          </mat-card-content>

          <mat-paginator
            [length]="totalCount"
            [pageSize]="pageSize"
            [pageSizeOptions]="[10, 25, 50]"
            (page)="onPage($event)"
            showFirstLastButtons>
          </mat-paginator>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .member-search-container {
      max-width: 1200px;
      margin: 0 auto;
    }

    .page-title {
      margin: 0 0 24px 0;
      font-size: 24px;
      font-weight: 500;
    }

    .search-card {
      margin-bottom: 24px;
    }

    .search-field {
      width: 100%;
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

    .table-card {
      overflow: hidden;
    }

    .table-content {
      padding: 0;
    }

    .members-table {
      width: 100%;
    }

    .clickable-row {
      cursor: pointer;
    }

    .clickable-row:hover {
      background-color: rgba(0, 0, 0, 0.04);
    }
  `],
})
export class MemberSearchComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  searchControl = new FormControl('');
  members: MemberRow[] = [];
  displayedColumns = ['externalMemberNumber', 'firstName', 'lastName', 'status', 'dob'];
  totalCount = 0;
  pageSize = 10;
  currentPage = 0;
  loading = false;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private apollo: Apollo,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        takeUntil(this.destroy$),
      )
      .subscribe(() => {
        this.currentPage = 0;
        this.fetchMembers();
      });

    this.fetchMembers();
  }

  ngAfterViewInit(): void {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchMembers(): void {
    this.loading = true;
    this.error = null;

    const searchTerm = this.searchControl.value?.trim() || '';
    const filter = searchTerm ? { name: searchTerm } : null;

    this.apollo
      .query<MembersListResult>({
        query: MEMBERS_LIST_QUERY,
        variables: {
          filter,
          page: {
            skip: this.currentPage * this.pageSize,
            take: this.pageSize,
          },
          sortField: 'LAST_NAME',
          sortDir: 'ASC',
        },
        fetchPolicy: 'network-only',
      })
      .subscribe({
        next: ({ data }) => {
          this.members = data?.members?.items ?? [];
          this.totalCount = data?.members?.totalCount ?? 0;
          this.loading = false;
        },
        error: (err) => {
          this.error = err?.message || 'Failed to load members.';
          this.loading = false;
        },
      });
  }

  onPage(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.fetchMembers();
  }

  goToMember(id: string): void {
    this.router.navigate(['/members', id]);
  }
}
