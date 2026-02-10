import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: false,
  template: `
    <div class="empty-state-container">
      <mat-icon class="empty-state-icon">{{ icon }}</mat-icon>
      <h3 *ngIf="title" class="empty-state-title">{{ title }}</h3>
      <p *ngIf="message" class="empty-state-message">{{ message }}</p>
    </div>
  `,
  styles: [`
    .empty-state-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 64px 16px;
      text-align: center;
    }
    .empty-state-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: rgba(0, 0, 0, 0.26);
      margin-bottom: 16px;
    }
    .empty-state-title {
      margin: 0 0 8px 0;
      font-size: 20px;
      font-weight: 500;
      color: rgba(0, 0, 0, 0.87);
    }
    .empty-state-message {
      margin: 0;
      font-size: 14px;
      color: rgba(0, 0, 0, 0.54);
      max-width: 400px;
    }
  `],
})
export class EmptyStateComponent {
  @Input() icon: string = 'inbox';
  @Input() title?: string;
  @Input() message?: string;
}
