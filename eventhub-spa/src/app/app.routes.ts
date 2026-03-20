import { Routes } from '@angular/router';
import { EventCreateComponent } from './events/components/event-create/event-create.component';
import { EventListComponent } from './events/components/event-list/event-list.component';

export const routes: Routes = [
  { path: 'events', component: EventListComponent },
  { path: 'events/create', component: EventCreateComponent },
  { path: '', pathMatch: 'full', redirectTo: 'events/create' },
];
