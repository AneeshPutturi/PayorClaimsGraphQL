import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { MemberSearchComponent } from './member-search.component';
import { MemberDetailComponent } from './member-detail.component';

const routes: Routes = [
  { path: '', component: MemberSearchComponent },
  { path: ':id', component: MemberDetailComponent },
];

@NgModule({
  imports: [
    SharedModule,
    RouterModule.forChild(routes),
  ],
  declarations: [
    MemberSearchComponent,
    MemberDetailComponent,
  ],
})
export class Member360Module {}
