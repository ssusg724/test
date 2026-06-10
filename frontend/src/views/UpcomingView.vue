<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api, statusLabel, type Live } from '../api';

const lives = ref<Live[]>([]);
const error = ref('');
const today = new Date(); today.setHours(0, 0, 0, 0);

function daysUntil(dateStr: string): number {
  const d = new Date(dateStr); d.setHours(0, 0, 0, 0);
  return Math.round((d.getTime() - today.getTime()) / 86400000);
}
function countdownLabel(dateStr: string): string {
  const n = daysUntil(dateStr);
  if (n === 0) return '今日！';
  if (n < 0) return `${-n}日前に終了`;
  if (n === 1) return '明日';
  return `あと ${n} 日`;
}

const sorted = computed(() =>
  [...lives.value].sort((a, b) => a.date.localeCompare(b.date)));

async function load() {
  try {
    const [applied, interested] = await Promise.all([
      api.lives({ status: 'Applied' }),
      api.lives({ status: 'Interested' }),
    ]);
    lives.value = [...applied, ...interested];
  } catch (e: any) { error.value = e.message; }
}
onMounted(load);
</script>

<template>
  <h1>⭐ 気になる・参戦予定</h1>
  <p class="muted">応募済み／気になるライブをカウントダウン付きで。</p>
  <p v-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>
  <p v-else-if="sorted.length === 0" class="muted">予定なし。ライブ登録時に「応募済み」「気になる」を選ぶとここに出ます。</p>

  <div style="display:grid; gap:12px;">
    <RouterLink v-for="l in sorted" :key="l.id" :to="`/lives/${l.id}`" style="color:inherit;">
      <div class="card row" style="justify-content:space-between;"
        :class="{ today: daysUntil(l.date) === 0, past: daysUntil(l.date) < 0 }">
        <div>
          <strong style="font-size:17px;">{{ l.title }}</strong>
          <div class="muted" style="margin-top:4px;">🎤 {{ l.artistName }} ・ 🏟 {{ l.venueName }} ・ 🗓 {{ l.date }}</div>
        </div>
        <div style="text-align:right;">
          <div class="cd">{{ countdownLabel(l.date) }}</div>
          <span class="badge" :class="l.status">{{ statusLabel[l.status] }}</span>
        </div>
      </div>
    </RouterLink>
  </div>
</template>

<style scoped>
.cd { font-size:18px; font-weight:800; color:var(--accent-2); margin-bottom:4px; }
.today { border-color:var(--accent); }
.today .cd { color:var(--accent); }
.past { opacity:.55; }
</style>
