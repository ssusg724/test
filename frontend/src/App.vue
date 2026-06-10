<script setup lang="ts">
import { ref, watch } from 'vue';
import { RouterLink, RouterView, useRouter } from 'vue-router';
import { api, type SearchHit } from './api';
import { theme, toggleTheme } from './theme';

const router = useRouter();
const q = ref('');
const hits = ref<SearchHit[]>([]);
const open = ref(false);
let timer: ReturnType<typeof setTimeout> | undefined;
let seq = 0;

const ICON: Record<SearchHit['type'], string> = { live: '🎟', artist: '🎤', venue: '🏟', song: '🎵' };
const ROUTE: Record<SearchHit['type'], (id: number) => string> = {
  live: id => `/lives/${id}`,
  artist: () => `/artists`,
  venue: () => `/venues`,
  song: () => `/songs`,
};

watch(q, (val) => {
  clearTimeout(timer);
  if (!val.trim()) { hits.value = []; open.value = false; return; }
  const mine = ++seq;
  timer = setTimeout(async () => {
    try {
      const r = await api.search(val.trim());
      if (mine !== seq) return; // 後発のクエリが走っていれば古い結果は破棄
      hits.value = r; open.value = true;
    } catch { if (mine === seq) hits.value = []; }
  }, 250);
});

function go(h: SearchHit) {
  open.value = false; q.value = ''; hits.value = []; seq++;
  router.push(ROUTE[h.type](h.id));
}
function onBlur() {
  // クリック(mousedown)を拾えるよう少し遅延してから閉じる
  setTimeout(() => { open.value = false; }, 150);
}
</script>

<template>
  <header class="topbar">
    <RouterLink to="/" class="brand">🎸 LiveLog</RouterLink>
    <nav>
      <RouterLink to="/" active-class="nav-inactive" exact-active-class="router-link-active">ホーム</RouterLink>
      <RouterLink to="/lives">ライブ</RouterLink>
      <RouterLink to="/upcoming">予定</RouterLink>
      <RouterLink to="/artists">バンド</RouterLink>
      <RouterLink to="/venues">会場</RouterLink>
      <RouterLink to="/songs">曲</RouterLink>
      <RouterLink to="/poster">画像</RouterLink>
    </nav>

    <div class="search">
      <input v-model="q" placeholder="🔍 検索…" @focus="open = hits.length > 0" @blur="onBlur" />
      <div v-if="open && hits.length" class="results">
        <button v-for="h in hits" :key="h.type + h.id" class="hit" @mousedown.prevent="go(h)">
          <span>{{ ICON[h.type] }}</span>
          <span class="hit-label">{{ h.label }}</span>
          <span class="muted hit-sub">{{ h.sub }}</span>
        </button>
      </div>
    </div>

    <button class="ghost icon" @click="toggleTheme" :title="theme === 'dark' ? 'ライト' : 'ダーク'">
      {{ theme === 'dark' ? '☀️' : '🌙' }}
    </button>
    <RouterLink to="/settings" class="icon-link" title="設定">⚙️</RouterLink>
    <RouterLink to="/lives/new"><button class="primary">+ 記録</button></RouterLink>
  </header>
  <main class="container">
    <RouterView />
  </main>
</template>

<style scoped>
.topbar {
  display: flex; align-items: center; gap: 14px;
  padding: 12px 20px; border-bottom: 1px solid var(--border);
  background: var(--surface); position: sticky; top: 0; z-index: 20; flex-wrap: wrap;
}
.brand { font-size: 19px; font-weight: 800; letter-spacing: .5px; color: var(--text); }
nav { display: flex; gap: 14px; }
nav a { color: var(--muted); font-weight: 600; font-size: 14px; }
nav a.router-link-active { color: var(--text); border-bottom: 2px solid var(--accent); padding-bottom: 2px; }
.search { position: relative; margin-left: auto; min-width: 180px; }
.search input { padding: 6px 10px; }
.results {
  position: absolute; top: 110%; left: 0; right: 0; z-index: 30;
  background: var(--surface); border: 1px solid var(--border); border-radius: 10px;
  overflow: hidden; box-shadow: 0 8px 24px rgba(0,0,0,.3);
}
.hit {
  display: flex; align-items: center; gap: 8px; width: 100%; text-align: left;
  background: transparent; border: none; border-bottom: 1px solid var(--border); border-radius: 0; padding: 8px 12px;
}
.hit:last-child { border-bottom: none; }
.hit:hover { background: var(--surface-2); }
.hit-label { font-weight: 600; }
.hit-sub { font-size: 12px; margin-left: auto; }
.icon, .icon-link { font-size: 16px; padding: 6px 10px; }
.icon-link { display: inline-flex; align-items: center; text-decoration: none; }
.container { max-width: 880px; margin: 0 auto; padding: 24px; }
</style>
