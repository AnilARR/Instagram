import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/feed/feed-page').then((m) => m.FeedPage),
    title: 'Instagram Dummy',
  },
  {
    path: 'explore',
    loadComponent: () => import('./features/explore/explore-page').then((m) => m.ExplorePage),
    title: 'Explore · Instagram Dummy',
  },
  {
    path: 'u/:username',
    loadComponent: () => import('./features/profile/profile-page').then((m) => m.ProfilePage),
    title: 'Profile · Instagram Dummy',
  },
  { path: '**', redirectTo: '' },
];
