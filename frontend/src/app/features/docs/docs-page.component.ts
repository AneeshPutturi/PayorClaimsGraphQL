import { Component } from '@angular/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-docs-page',
  standalone: false,
  template: `
    <div class="docs-container">
      <h2 class="page-title">API Documentation</h2>

      <!-- GraphQL Explorer -->
      <mat-card class="docs-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>explore</mat-icon>
          <mat-card-title>GraphQL Explorer</mat-card-title>
          <mat-card-subtitle>Interactive query builder powered by Altair</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>Use the Altair GraphQL Client to explore the schema, build queries, and test mutations interactively.</p>
        </mat-card-content>
        <mat-card-actions>
          <a mat-raised-button color="primary" [href]="altairUrl" target="_blank">
            <mat-icon>open_in_new</mat-icon>
            Open Altair Explorer
          </a>
        </mat-card-actions>
      </mat-card>

      <!-- Sample Queries -->
      <mat-card class="docs-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>code</mat-icon>
          <mat-card-title>Sample Queries</mat-card-title>
        </mat-card-header>
        <mat-card-content>

          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>Member360 Query</mat-panel-title>
              <mat-panel-description>Full member profile with claims, EOBs, and payments</mat-panel-description>
            </mat-expansion-panel-header>
            <pre><code>query Member360($id: UUID!) {{ '{' }}
  member(id: $id) {{ '{' }}
    id
    firstName
    lastName
    dob
    status
    addresses {{ '{' }}
      line1
      city
      state
      zip
    {{ '}' }}
    claims {{ '{' }}
      id
      claimNumber
      status
      totalAmount
      lineItems {{ '{' }}
        procedureCode
        amount
      {{ '}' }}
    {{ '}' }}
    eobs {{ '{' }}
      id
      generatedDate
      totalPaid
    {{ '}' }}
  {{ '}' }}
{{ '}' }}</code></pre>
          </mat-expansion-panel>

          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>Submit Claim Mutation</mat-panel-title>
              <mat-panel-description>Create a new healthcare claim</mat-panel-description>
            </mat-expansion-panel-header>
            <pre><code>mutation SubmitClaim($input: SubmitClaimInput!) {{ '{' }}
  submitClaim(input: $input) {{ '{' }}
    id
    claimNumber
    status
    totalAmount
    lineItems {{ '{' }}
      procedureCode
      amount
      status
    {{ '}' }}
  {{ '}' }}
{{ '}' }}

# Variables:
{{ '{' }}
  "input": {{ '{' }}
    "memberId": "...",
    "providerId": "...",
    "lineItems": [
      {{ '{' }}
        "procedureCode": "99213",
        "amount": 150.00,
        "diagnosisCodes": ["J06.9"]
      {{ '}' }}
    ]
  {{ '}' }}
{{ '}' }}</code></pre>
          </mat-expansion-panel>

          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>Adjudicate Claim Mutation</mat-panel-title>
              <mat-panel-description>Process and adjudicate a pending claim</mat-panel-description>
            </mat-expansion-panel-header>
            <pre><code>mutation AdjudicateClaim($input: AdjudicateClaimInput!) {{ '{' }}
  adjudicateClaim(input: $input) {{ '{' }}
    id
    claimNumber
    status
    adjudicatedDate
    lineItems {{ '{' }}
      procedureCode
      status
      allowedAmount
      paidAmount
    {{ '}' }}
  {{ '}' }}
{{ '}' }}

# Variables:
{{ '{' }}
  "input": {{ '{' }}
    "claimId": "...",
    "rowVersion": "...",
    "lineItems": [
      {{ '{' }}
        "lineItemId": "...",
        "decision": "APPROVED",
        "allowedAmount": 120.00,
        "paidAmount": 96.00
      {{ '}' }}
    ]
  {{ '}' }}
{{ '}' }}</code></pre>
          </mat-expansion-panel>

          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>Export Claims</mat-panel-title>
              <mat-panel-description>Request an async export of member claims</mat-panel-description>
            </mat-expansion-panel-header>
            <pre><code># Step 1: Request export
mutation RequestExport($memberId: UUID!) {{ '{' }}
  requestMemberClaimsExport(memberId: $memberId) {{ '{' }}
    jobId
    status
  {{ '}' }}
{{ '}' }}

# Step 2: Poll for status
query ExportJob($jobId: UUID!) {{ '{' }}
  exportJob(jobId: $jobId) {{ '{' }}
    jobId
    status
    downloadTokenOnce
  {{ '}' }}
{{ '}' }}</code></pre>
          </mat-expansion-panel>

        </mat-card-content>
      </mat-card>

      <!-- Troubleshooting -->
      <mat-card class="docs-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>build</mat-icon>
          <mat-card-title>Troubleshooting</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>401 / 403 Errors</mat-panel-title>
            </mat-expansion-panel-header>
            <p>Ensure valid JWT token with correct roles. Tokens expire after a set period — re-authenticate
              if you receive unauthorized errors. The required roles for each operation are enforced at the
              field level via the <code>[Authorize]</code> directive.</p>
          </mat-expansion-panel>

          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>Persisted Queries</mat-panel-title>
            </mat-expansion-panel-header>
            <p>If you receive a <code>PERSISTED_ONLY</code> or <code>UNKNOWN_HASH</code> error, the server
              is configured to accept only persisted (pre-approved) queries. Set
              <code>PersistedQueriesEnabled=false</code> in dev to allow arbitrary queries during development.</p>
          </mat-expansion-panel>

          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>Query Depth / Complexity Limits</mat-panel-title>
            </mat-expansion-panel-header>
            <p>The server enforces maximum query depth and complexity to prevent abuse. If you receive a
              <code>QUERY_TOO_DEEP</code> or <code>QUERY_TOO_COMPLEX</code> error, simplify nested queries
              or increase <code>MaxDepth</code> / <code>MaxComplexity</code> in the server configuration.</p>
          </mat-expansion-panel>

          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>Concurrency Conflicts</mat-panel-title>
            </mat-expansion-panel-header>
            <p>If you encounter a <code>CONCURRENCY_CONFLICT</code> error, the entity was modified by another
              user since you last fetched it. Refetch the entity to get the latest <code>rowVersion</code>
              and retry your mutation with the updated value.</p>
          </mat-expansion-panel>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .docs-container {
      max-width: 900px;
      margin: 0 auto;
    }

    .page-title {
      margin: 0 0 24px 0;
      font-size: 24px;
      font-weight: 500;
    }

    .docs-card {
      margin-bottom: 24px;
    }

    mat-expansion-panel {
      margin-bottom: 8px;
    }

    pre {
      background-color: #263238;
      color: #eeffff;
      padding: 16px;
      border-radius: 8px;
      overflow-x: auto;
      font-size: 0.85rem;
      line-height: 1.5;
      margin: 8px 0;
    }

    code {
      font-family: 'Fira Code', 'Consolas', 'Monaco', monospace;
    }

    p code {
      background-color: rgba(0, 0, 0, 0.06);
      padding: 2px 6px;
      border-radius: 4px;
      font-size: 0.875rem;
    }
  `],
})
export class DocsPageComponent {
  altairUrl = environment.altairUrl;
}
