import { createRouter, createWebHistory } from 'vue-router';

const routes = [
  { path: '/', component: () => import('./views/DashboardView.vue') },
  { path: '/lives', component: () => import('./views/LivesView.vue') },
  { path: '/lives/new', component: () => import('./views/LiveFormView.vue') },
  { path: '/lives/:id', component: () => import('./views/LiveDetailView.vue'), props: true },
  { path: '/lives/:id/edit', component: () => import('./views/LiveFormView.vue'), props: true },
  { path: '/venues', component: () => import('./views/VenuesView.vue') },
  { path: '/artists', component: () => import('./views/ArtistsView.vue') },
  { path: '/songs', component: () => import('./views/SongsView.vue') },
  { path: '/upcoming', component: () => import('./views/UpcomingView.vue') },
  { path: '/poster', component: () => import('./views/PosterView.vue') },
  { path: '/settings', component: () => import('./views/SettingsView.vue') },
];

export default createRouter({
  history: createWebHistory(),
  routes,
});
