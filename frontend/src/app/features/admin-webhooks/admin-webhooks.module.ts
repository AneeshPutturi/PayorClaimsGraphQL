import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { WebhooksPageComponent } from './webhooks-page.component';

const routes: Routes = [
  { path: '', component: WebhooksPageComponent },
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    SharedModule,
    ReactiveFormsModule,
  ],
  declarations: [
    WebhooksPageComponent,
  ],
})
export class AdminWebhooksModule {}
