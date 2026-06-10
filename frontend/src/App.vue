<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router';
</script>

<template>
  <header class="topbar">
    <div class="brand">🎸 LiveLog</div>
    <nav>
      <RouterLink to="/" active-class="nav-inactive" exact-active-class="router-link-active">ホーム</RouterLink>
      <RouterLink to="/lives">ライブ</RouterLink>
      <RouterLink to="/artists">バンド</RouterLink>
      <RouterLink to="/venues">会場</RouterLink>
      <RouterLink to="/songs">曲を探す</RouterLink>
      <RouterLink to="/poster">画像メーカー</RouterLink>
    </nav>
    <RouterLink to="/lives/new"><button class="primary">+ 記録する</button></RouterLink>
  </header>
  <main class="container">
    <RouterView />
  </main>
</template>

<style scoped>
.topbar {
  display: flex; align-items: center; gap: 20px;
  padding: 14px 24px; border-bottom: 1px solid var(--border);
  background: var(--surface); position: sticky; top: 0; z-index: 10;
}
.brand { font-size: 20px; font-weight: 800; letter-spacing: .5px; }
nav { display: flex; gap: 18px; margin-right: auto; }
nav a { color: var(--muted); font-weight: 600; }
nav a.router-link-active { color: var(--text); border-bottom: 2px solid var(--accent); padding-bottom: 2px; }
.container { max-width: 880px; margin: 0 auto; padding: 24px; }
</style>
