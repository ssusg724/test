<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api, type Stats } from '../api';

const stats = ref<Stats | null>(null);
const error = ref('');
const maxYear = computed(() =>
  Math.max(1, ...(stats.value?.byYear.map(y => y.count) ?? [1])));

onMounted(async () => {
  try { stats.value = await api.stats(); }
  catch (e: any) { error.value = `読み込み失敗: ${e.message}（バックエンドは起動してる？）`; }
});
</script>

<template>
  <h1>📊 参戦ダッシュボード</h1>
  <p v-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>

  <template v-else-if="stats">
    <div class="stat-grid">
      <div class="card stat"><div class="num">{{ stats.totalAttended }}</div><div class="muted">通算参戦</div></div>
      <div class="card stat"><div class="num">{{ stats.thisYearAttended }}</div><div class="muted">今年</div></div>
      <div class="card stat"><div class="num">{{ stats.upcomingCount }}</div><div class="muted">予定/気になる</div></div>
      <div class="card stat"><div class="num">{{ stats.uniqueSongsHeard }}</div><div class="muted">生で聴いた曲</div></div>
    </div>

    <div class="two-col">
      <div class="card">
        <h3 style="margin-top:0;">年別参戦数</h3>
        <p v-if="stats.byYear.length === 0" class="muted">データなし</p>
        <div v-for="y in stats.byYear" :key="y.year" class="barrow">
          <span class="ylabel">{{ y.year }}</span>
          <div class="bar" :style="{ width: (y.count / maxYear * 100) + '%' }"></div>
          <span class="ycount">{{ y.count }}</span>
        </div>
      </div>

      <div class="card">
        <h3 style="margin-top:0;">🏟 よく行く会場</h3>
        <p v-if="stats.topVenues.length === 0" class="muted">データなし</p>
        <div v-for="v in stats.topVenues" :key="v.name" class="row" style="justify-content:space-between; padding:4px 0;">
          <span>{{ v.name }}</span><span class="badge">{{ v.count }}</span>
        </div>
        <h3>🎤 よく観るバンド</h3>
        <div v-for="a in stats.topArtists" :key="a.name" class="row" style="justify-content:space-between; padding:4px 0;">
          <span>{{ a.name }}</span><span class="badge">{{ a.count }}</span>
        </div>
      </div>
    </div>

    <div class="card cta">
      <div>
        <strong>🎨 「私が観たバンド」画像を作る</strong>
        <div class="muted">参戦履歴からX映えする画像を生成できます</div>
      </div>
      <RouterLink to="/poster"><button class="primary">作成する</button></RouterLink>
    </div>
  </template>
</template>

<style scoped>
.stat-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:12px; margin-bottom:16px; }
.stat { text-align:center; }
.stat .num { font-size:34px; font-weight:800; color:var(--accent); }
.two-col { display:grid; grid-template-columns:1fr 1fr; gap:12px; }
.barrow { display:flex; align-items:center; gap:10px; margin:6px 0; }
.ylabel { width:48px; color:var(--muted); font-size:13px; }
.ycount { width:24px; text-align:right; }
.bar { height:18px; background:linear-gradient(90deg,var(--accent),var(--accent-2)); border-radius:4px; min-width:4px; }
.cta { display:flex; justify-content:space-between; align-items:center; margin-top:16px; }
@media (max-width:680px){ .stat-grid{grid-template-columns:repeat(2,1fr);} .two-col{grid-template-columns:1fr;} }
</style>
