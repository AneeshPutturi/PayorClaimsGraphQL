import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading',
  standalone: false,
  template: `
    <div class="loading-container">
      <mat-spinner diameter="48"></mat-spinner>
      <p *ngIf="message" class="loading-message">{{ message }}</p>
    </div>
  `,
  styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 48px 16px;
      gap: 16px;
    }
    .loading-message {
      margin: 0;
      color: rgba(0, 0, 0, 0.54);
      font-size: 14px;
    }
  `],
})
export class LoadingComponent {
  @Input() message?: string;
}
