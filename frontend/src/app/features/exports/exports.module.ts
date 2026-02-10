import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ExportPageComponent } from './export-page.component';

const routes: Routes = [
  { path: '', component: ExportPageComponent },
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    SharedModule,
  ],
  declarations: [
    ExportPageComponent,
  ],
})
export class ExportsModule {}
