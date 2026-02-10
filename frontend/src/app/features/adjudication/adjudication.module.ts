import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';

import { WorkQueueComponent } from './work-queue.component';
import { AdjudicationEditorComponent } from './adjudication-editor.component';

const routes: Routes = [
  { path: '', component: WorkQueueComponent },
  { path: ':id', component: AdjudicationEditorComponent },
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    SharedModule,
    ReactiveFormsModule,
  ],
  declarations: [
    WorkQueueComponent,
    AdjudicationEditorComponent,
  ],
})
export class AdjudicationModule {}
