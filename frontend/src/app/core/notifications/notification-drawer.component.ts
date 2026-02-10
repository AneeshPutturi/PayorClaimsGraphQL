import { Component } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService, AppNotification } from './notification.service';

@Component({
  selector: 'app-notification-drawer',
  standalone: false,
  template: `
    <div class="notification-wrapper">
      <button mat-icon-button
              (click)="open = !open"
              aria-label="Notifications"
              [matBadge]="notificationService.unreadCount()"
              [matBadgeHidden]="notificationService.unreadCount() === 0"
              matBadgeColor="warn"
              matBadgeSize="small">
        <mat-icon>notifications</mat-icon>
      </button>

      @if (open) {
        <div class="notification-backdrop" (click)="open = false"></div>
        <div class="notification-panel mat-elevation-z8">
          <div class="panel-header">
            <span class="panel-title">Notifications</span>
            <span class="panel-count">{{ notificationService.unreadCount() }}</span>
          </div>

          @if (notificationService.notifications().length === 0) {
            <div class="empty-state">
              <mat-icon>notifications_none</mat-icon>
              <span>No notifications</span>
            </div>
          }

          <div class="notification-list">
            @for (n of notificationService.notifications(); track n.id) {
              <div class="notification-item" (click)="navigate(n)">
                <div class="notification-icon">
                  @if (n.type === 'claim-status') {
                    <mat-icon color="primary">description</mat-icon>
                  } @else {
                    <mat-icon color="accent">receipt_long</mat-icon>
                  }
                </div>
                <div class="notification-body">
                  <span class="notification-message">{{ n.message }}</span>
                  <span class="notification-time">{{ formatDate(n.timestamp) }}</span>
                </div>
                <button mat-icon-button
                        (click)="dismiss($event, n.id)"
                        aria-label="Dismiss"
                        class="dismiss-btn">
                  <mat-icon>close</mat-icon>
                </button>
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .notification-wrapper { position: relative; display: inline-block; }
    .notification-backdrop { position: fixed; top: 0; left: 0; right: 0; bottom: 0; z-index: 999; }
    .notification-panel { position: absolute; top: 48px; right: 0; width: 380px; max-height: 480px; background: white; border-radius: 8px; z-index: 1000; overflow: hidden; display: flex; flex-direction: column; }
    .panel-header { display: flex; align-items: center; justify-content: space-between; padding: 16px; border-bottom: 1px solid rgba(0, 0, 0, 0.08); }
    .panel-title { font-size: 1rem; font-weight: 500; }
    .panel-count { background: #3f51b5; color: white; font-size: 0.75rem; font-weight: 600; padding: 2px 8px; border-radius: 12px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 32px 16px; color: rgba(0, 0, 0, 0.38); }
    .notification-list { overflow-y: auto; max-height: 400px; }
    .notification-item { display: flex; align-items: flex-start; gap: 12px; padding: 12px 16px; cursor: pointer; border-bottom: 1px solid rgba(0, 0, 0, 0.04); transition: background-color 0.15s ease; }
    .notification-item:hover { background-color: rgba(0, 0, 0, 0.04); }
    .notification-icon { padding-top: 2px; }
    .notification-body { flex: 1; display: flex; flex-direction: column; gap: 4px; }
    .notification-message { font-size: 0.875rem; line-height: 1.4; color: rgba(0, 0, 0, 0.87); }
    .notification-time { font-size: 0.75rem; color: rgba(0, 0, 0, 0.54); }
    .dismiss-btn { margin-top: -4px; }
  `],
})
export class NotificationDrawerComponent {
  open = false;
  private datePipe = new DatePipe('en-US');

  constructor(
    public notificationService: NotificationService,
    private router: Router,
  ) {}

  formatDate(date: Date): string {
    return this.datePipe.transform(date, 'short') ?? '';
  }

  navigate(notification: AppNotification): void {
    this.open = false;
    if (notification.data['claimId']) {
      this.router.navigate(['/claims', notification.data['claimId']]);
    }
  }

  dismiss(event: Event, id: string): void {
    event.stopPropagation();
    this.notificationService.dismiss(id);
  }
}
