import { Injectable, computed, signal } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { Subscription } from 'rxjs';

const ON_CLAIM_STATUS_CHANGED = gql`
  subscription OnClaimStatusChanged($claimId: UUID!) {
    onClaimStatusChanged(claimId: $claimId) {
      claimId
      claimNumber
      oldStatus
      newStatus
      changedAt
    }
  }
`;

const ON_EOB_GENERATED = gql`
  subscription OnEobGenerated($memberId: UUID!) {
    onEobGenerated(memberId: $memberId) {
      eobId
      memberId
      claimId
      generatedDate
      totalPaid
    }
  }
`;

export interface AppNotification {
  id: string;
  type: 'claim-status' | 'eob-generated';
  message: string;
  timestamp: Date;
  data: Record<string, unknown>;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  readonly notifications = signal<AppNotification[]>([]);
  readonly unreadCount = computed(() => this.notifications().length);

  private subscriptions = new Map<string, Subscription>();
  private nextId = 1;

  constructor(private apollo: Apollo) {}

  subscribeToClaimChanges(claimId: string): void {
    const key = `claim-${claimId}`;
    if (this.subscriptions.has(key)) return;

    const sub = this.apollo
      .subscribe<{
        onClaimStatusChanged: {
          claimId: string;
          claimNumber: string;
          oldStatus: string;
          newStatus: string;
          changedAt: string;
        };
      }>({
        query: ON_CLAIM_STATUS_CHANGED,
        variables: { claimId },
      })
      .subscribe({
        next: ({ data }) => {
          if (data?.onClaimStatusChanged) {
            const evt = data.onClaimStatusChanged;
            this.pushNotification({
              type: 'claim-status',
              message: `Claim ${evt.claimNumber} changed from ${evt.oldStatus} to ${evt.newStatus}`,
              data: { claimId: evt.claimId, claimNumber: evt.claimNumber },
            });
          }
        },
      });

    this.subscriptions.set(key, sub);
  }

  subscribeToEobs(memberId: string): void {
    const key = `eob-${memberId}`;
    if (this.subscriptions.has(key)) return;

    const sub = this.apollo
      .subscribe<{
        onEobGenerated: {
          eobId: string;
          memberId: string;
          claimId: string;
          generatedDate: string;
          totalPaid: number;
        };
      }>({
        query: ON_EOB_GENERATED,
        variables: { memberId },
      })
      .subscribe({
        next: ({ data }) => {
          if (data?.onEobGenerated) {
            const evt = data.onEobGenerated;
            this.pushNotification({
              type: 'eob-generated',
              message: `New EOB generated for claim ${evt.claimId} — $${evt.totalPaid.toFixed(2)} paid`,
              data: { eobId: evt.eobId, memberId: evt.memberId, claimId: evt.claimId },
            });
          }
        },
      });

    this.subscriptions.set(key, sub);
  }

  dismiss(id: string): void {
    this.notifications.update((list) => list.filter((n) => n.id !== id));
  }

  unsubscribe(key: string): void {
    const sub = this.subscriptions.get(key);
    if (sub) {
      sub.unsubscribe();
      this.subscriptions.delete(key);
    }
  }

  unsubscribeAll(): void {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
    this.subscriptions.clear();
  }

  private pushNotification(partial: Omit<AppNotification, 'id' | 'timestamp'>): void {
    const notification: AppNotification = {
      id: String(this.nextId++),
      timestamp: new Date(),
      ...partial,
    };
    this.notifications.update((list) => [notification, ...list]);
  }
}
