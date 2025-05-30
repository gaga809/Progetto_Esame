import { Routes } from '@angular/router';
import { HomeComponent } from './views/home/home.component';
import { ScoresComponent } from './views/scores/scores.component';
import { RegisterComponent } from './views/register/register.component';

export const routes: Routes = [
  {
    path: 'home',
    component: HomeComponent
  },
  {
    path: 'scores',
    component: ScoresComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: '**',
    redirectTo: 'home'
  }
];
