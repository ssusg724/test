import { ref } from 'vue';

export type Theme = 'dark' | 'light';
const KEY = 'livelog-theme';

export const theme = ref<Theme>((localStorage.getItem(KEY) as Theme) || 'dark');

export function applyTheme(t: Theme) {
  theme.value = t;
  localStorage.setItem(KEY, t);
  document.documentElement.setAttribute('data-theme', t);
}

export function toggleTheme() {
  applyTheme(theme.value === 'dark' ? 'light' : 'dark');
}

// 起動時に保存済みテーマを反映
applyTheme(theme.value);
