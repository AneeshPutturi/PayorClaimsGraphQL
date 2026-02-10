import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Apollo, gql } from 'apollo-angular';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

const REGISTER_WEBHOOK_MUTATION = gql`
  mutation RegisterWebhook($input: RegisterWebhookInput!) {
    registerWebhook(input: $input) {
      id
      name
      url
      isActive
    }
  }
`;

const DEACTIVATE_WEBHOOK_MUTATION = gql`
  mutation DeactivateWebhook($id: UUID!) {
    deactivateWebhook(id: $id) {
      id
      name
      isActive
    }
  }
`;

interface WebhookResult {
  id: string;
  name: string;
  url: string;
  isActive: boolean;
}

@Component({
  selector: 'app-webhooks-page',
  standalone: false,
  template: `
    <div class="webhooks-container">
      <h2 class="page-title">Webhook Management</h2>

      <!-- Register Webhook -->
      <mat-card class="register-card">
        <mat-card-header>
          <mat-card-title>Register Webhook</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="registerForm" (ngSubmit)="registerWebhook()" class="register-form">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Name</mat-label>
              <input matInput formControlName="name" placeholder="e.g. Claim Status Notifier" />
              <mat-error>Name is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>URL</mat-label>
              <input matInput formControlName="url" placeholder="https://example.com/webhook" />
              <mat-error>A valid URL is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Secret (optional)</mat-label>
              <input matInput formControlName="secret" placeholder="Optional shared secret" />
            </mat-form-field>

            <button mat-raised-button
                    color="primary"
                    type="submit"
                    [disabled]="registerForm.invalid || registerLoading">
              <mat-icon>add</mat-icon>
              Register Webhook
            </button>
          </form>
        </mat-card-content>
      </mat-card>

      @if (registerLoading) {
        <app-loading message="Registering webhook…"></app-loading>
      }

      @if (registerError) {
        <mat-card class="message-card error-card">
          <mat-card-content>
            <div class="message-content">
              <mat-icon color="warn">error_outline</mat-icon>
              <span>{{ registerError }}</span>
            </div>
          </mat-card-content>
        </mat-card>
      }

      @if (registerSuccess) {
        <mat-card class="message-card success-card">
          <mat-card-content>
            <div class="message-content">
              <mat-icon class="success-icon">check_circle</mat-icon>
              <span>{{ registerSuccess }}</span>
            </div>
          </mat-card-content>
        </mat-card>
      }

      <!-- Created Webhook Details -->
      @if (createdWebhook) {
        <mat-card class="result-card">
          <mat-card-header>
            <mat-card-title>Created Webhook</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="detail-row">
              <span class="label">ID:</span>
              <span>{{ createdWebhook.id }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Name:</span>
              <span>{{ createdWebhook.name }}</span>
            </div>
            <div class="detail-row">
              <span class="label">URL:</span>
              <span>{{ createdWebhook.url }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Active:</span>
              <app-status-badge [status]="createdWebhook.isActive ? 'Active' : 'Inactive'"></app-status-badge>
            </div>
          </mat-card-content>
        </mat-card>
      }

      <!-- Deactivate Webhook -->
      <mat-card class="deactivate-card">
        <mat-card-header>
          <mat-card-title>Deactivate Webhook</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="deactivate-row">
            <mat-form-field appearance="outline" class="deactivate-field">
              <mat-label>Webhook ID</mat-label>
              <input matInput [(ngModel)]="deactivateId" placeholder="Enter webhook UUID" />
            </mat-form-field>
            <button mat-raised-button
                    color="warn"
                    (click)="deactivateWebhook()"
                    [disabled]="!deactivateId.trim() || deactivateLoading">
              <mat-icon>block</mat-icon>
              Deactivate
            </button>
          </div>
        </mat-card-content>
      </mat-card>

      @if (deactivateLoading) {
        <app-loading message="Deactivating webhook…"></app-loading>
      }

      @if (deactivateError) {
        <mat-card class="message-card error-card">
          <mat-card-content>
            <div class="message-content">
              <mat-icon color="warn">error_outline</mat-icon>
              <span>{{ deactivateError }}</span>
            </div>
          </mat-card-content>
        </mat-card>
      }

      @if (deactivateSuccess) {
        <mat-card class="message-card success-card">
          <mat-card-content>
            <div class="message-content">
              <mat-icon class="success-icon">check_circle</mat-icon>
              <span>{{ deactivateSuccess }}</span>
            </div>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .webhooks-container {
      max-width: 800px;
      margin: 0 auto;
    }

    .page-title {
      margin: 0 0 24px 0;
      font-size: 24px;
      font-weight: 500;
    }

    .register-card,
    .deactivate-card,
    .result-card {
      margin-bottom: 24px;
    }

    .register-form {
      display: flex;
      flex-direction: column;
      gap: 4px;
      margin-top: 8px;
    }

    .full-width {
      width: 100%;
    }

    .deactivate-row {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      margin-top: 8px;
    }

    .deactivate-field {
      flex: 1;
    }

    .message-card {
      margin-bottom: 16px;
    }

    .message-content {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .success-icon {
      color: #2e7d32;
    }

    .error-card .message-content {
      color: rgba(0, 0, 0, 0.87);
    }

    .success-card .message-content {
      color: rgba(0, 0, 0, 0.87);
    }

    .detail-row {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 8px 0;
      border-bottom: 1px solid rgba(0, 0, 0, 0.06);
    }

    .detail-row:last-child {
      border-bottom: none;
    }

    .label {
      font-weight: 500;
      min-width: 60px;
    }
  `],
})
export class WebhooksPageComponent implements OnDestroy {
  registerForm: FormGroup;
  registerLoading = false;
  registerError: string | null = null;
  registerSuccess: string | null = null;
  createdWebhook: WebhookResult | null = null;

  deactivateId = '';
  deactivateLoading = false;
  deactivateError: string | null = null;
  deactivateSuccess: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private apollo: Apollo,
    private fb: FormBuilder,
  ) {
    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      url: ['', [Validators.required, Validators.pattern('https?://.+')]],
      secret: [''],
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  registerWebhook(): void {
    if (this.registerForm.invalid) return;

    this.registerLoading = true;
    this.registerError = null;
    this.registerSuccess = null;
    this.createdWebhook = null;

    const { name, url, secret } = this.registerForm.value;
    const input: Record<string, string> = { name, url };
    if (secret) {
      input['secret'] = secret;
    }

    this.apollo
      .mutate<{ registerWebhook: WebhookResult }>({
        mutation: REGISTER_WEBHOOK_MUTATION,
        variables: { input },
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ data }) => {
          this.registerLoading = false;
          if (data?.registerWebhook) {
            this.createdWebhook = data.registerWebhook;
            this.registerSuccess = `Webhook "${data.registerWebhook.name}" registered successfully.`;
            this.registerForm.reset();
          }
        },
        error: (err) => {
          this.registerLoading = false;
          this.registerError = err?.message || 'Failed to register webhook.';
        },
      });
  }

  deactivateWebhook(): void {
    const id = this.deactivateId.trim();
    if (!id) return;

    this.deactivateLoading = true;
    this.deactivateError = null;
    this.deactivateSuccess = null;

    this.apollo
      .mutate<{ deactivateWebhook: { id: string; name: string; isActive: boolean } }>({
        mutation: DEACTIVATE_WEBHOOK_MUTATION,
        variables: { id },
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ data }) => {
          this.deactivateLoading = false;
          if (data?.deactivateWebhook) {
            this.deactivateSuccess = `Webhook "${data.deactivateWebhook.name}" has been deactivated.`;
            this.deactivateId = '';
          }
        },
        error: (err) => {
          this.deactivateLoading = false;
          this.deactivateError = err?.message || 'Failed to deactivate webhook.';
        },
      });
  }
}
