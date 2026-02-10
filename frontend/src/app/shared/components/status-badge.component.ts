import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: false,
  template: `
    <mat-chip [ngClass]="colorClass" [highlighted]="true">
      {{ status }}
    </mat-chip>
  `,
  styles: [`
    :host {
      display: inline-block;
    }
    .status-green {
      --mdc-chip-elevated-container-color: #e8f5e9;
      --mdc-chip-label-text-color: #2e7d32;
    }
    .status-amber {
      --mdc-chip-elevated-container-color: #fff3e0;
      --mdc-chip-label-text-color: #e65100;
    }
    .status-red {
      --mdc-chip-elevated-container-color: #ffebee;
      --mdc-chip-label-text-color: #c62828;
    }
    .status-gray {
      --mdc-chip-elevated-container-color: #f5f5f5;
      --mdc-chip-label-text-color: #616161;
    }
  `],
})
export class StatusBadgeComponent {
  @Input() status: string = '';

  get colorClass(): string {
    const s = (this.status || '').toLowerCase();
    if (['active', 'paid', 'approved', 'ready', 'completed', 'succeeded'].includes(s)) {
      return 'status-green';
    }
    if (['pending', 'received', 'queued', 'running'].includes(s)) {
      return 'status-amber';
    }
    if (['denied', 'failed', 'inactive'].includes(s)) {
      return 'status-red';
    }
    return 'status-gray';
  }
}
