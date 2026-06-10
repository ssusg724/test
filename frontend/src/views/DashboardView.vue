<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api, type Stats } from '../api';

const stats = ref<Stats | null>(null);
const error = ref('');
const maxYear = computed(() =>
  Math.max(1, ...(stats.value?.byYear.map(y => y.count) ?? [1])));
const maxMonth = computed(() =>
  Math.max(1, ...(stats.value?.byMonthThisYear.map(m => m.count) ?? [1])));
const thisYear = new Date().getFullYear();

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
    <div class="stat-grid">
      <div class="card stat"><div class="num2">¥{{ stats.totalSpent.toLocaleString() }}</div><div class="muted">使った金額(参戦)</div></div>
      <div class="card stat"><div class="num2">{{ stats.averageRating != null ? stats.averageRating.toFixed(2) : '—' }}</div><div class="muted">平均評価</div></div>
      <div class="card stat" style="grid-column:span 2;">
        <div class="num2" style="font-size:20px;">{{ stats.favoriteSong ?? '—' }}</div>
        <div class="muted">いちばん聴いた曲{{ stats.favoriteSong ? `（${stats.favoriteSongCount}回）` : '' }}</div>
      </div>
    </div>

    <div class="card" style="margin-bottom:12px;">
      <h3 style="margin-top:0;">{{ thisYear }}年の月別参戦</h3>
      <div class="months">
        <div v-for="m in stats.byMonthThisYear" :key="m.month" class="mcol">
          <div class="mbar-wrap">
            <div class="mbar" :style="{ height: (m.count / maxMonth * 100) + '%' }" :title="`${m.count}本`"></div>
          </div>
          <div class="mlabel">{{ m.month }}</div>
        </div>
      </div>
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
        <h3>🎵 よく聴く曲</h3>
        <p v-if="stats.topSongs.length === 0" class="muted">データなし</p>
        <div v-for="s in stats.topSongs" :key="s.name" class="row" style="justify-content:space-between; padding:4px 0;">
          <span>{{ s.name }}</span><span class="badge">{{ s.count }}回</span>
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
.stat .num2 { font-size:26px; font-weight:800; color:var(--accent-2); }
.months { display:flex; gap:6px; align-items:flex-end; height:120px; }
.mcol { flex:1; display:flex; flex-direction:column; align-items:center; gap:4px; height:100%; }
.mbar-wrap { flex:1; width:100%; display:flex; align-items:flex-end; }
.mbar { width:100%; background:linear-gradient(180deg,var(--accent-2),var(--accent)); border-radius:4px 4px 0 0; min-height:2px; }
.mlabel { font-size:11px; color:var(--muted); }
.two-col { display:grid; grid-template-columns:1fr 1fr; gap:12px; }
.barrow { display:flex; align-items:center; gap:10px; margin:6px 0; }
.ylabel { width:48px; color:var(--muted); font-size:13px; }
.ycount { width:24px; text-align:right; }
.bar { height:18px; background:linear-gradient(90deg,var(--accent),var(--accent-2)); border-radius:4px; min-width:4px; }
.cta { display:flex; justify-content:space-between; align-items:center; margin-top:16px; }
@media (max-width:680px){ .stat-grid{grid-template-columns:repeat(2,1fr);} .two-col{grid-template-columns:1fr;} }
</style>
