import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { MatListModule } from '@angular/material/list';
import { SharedModule } from '../../shared/shared.module';

import { ClaimListComponent } from './claim-list.component';
import { ClaimDetailComponent } from './claim-detail.component';
import { ClaimSubmitComponent } from './claim-submit.component';

const routes: Routes = [
  { path: '', component: ClaimListComponent },
  { path: 'submit', component: ClaimSubmitComponent },
  { path: ':id', component: ClaimDetailComponent },
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    SharedModule,
    ReactiveFormsModule,
    MatListModule,
  ],
  declarations: [
    ClaimListComponent,
    ClaimDetailComponent,
    ClaimSubmitComponent,
  ],
})
export class ClaimsModule {}
