import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { DocsPageComponent } from './docs-page.component';

const routes: Routes = [
  { path: '', component: DocsPageComponent },
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    SharedModule,
  ],
  declarations: [
    DocsPageComponent,
  ],
})
export class DocsModule {}
