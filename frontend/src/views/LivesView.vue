<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { api, statusLabel, type Live, type LiveStatus } from '../api';

const lives = ref<Live[]>([]);
const loading = ref(true);
const error = ref('');
const filter = ref<LiveStatus | ''>('');

async function load() {
  loading.value = true;
  error.value = '';
  try {
    lives.value = await api.lives(filter.value ? { status: filter.value } : {});
  } catch (e: any) {
    error.value = `読み込み失敗: ${e.message}（バックエンドは起動してる？）`;
  } finally {
    loading.value = false;
  }
}
onMounted(load);
</script>

<template>
  <div class="row" style="justify-content: space-between; margin-bottom: 16px;">
    <h1 style="margin:0;">ライブ記録</h1>
    <div class="row">
      <select v-model="filter" @change="load" style="width:auto;">
        <option value="">すべて</option>
        <option value="Attended">参戦済み</option>
        <option value="Applied">応募済み</option>
        <option value="Interested">気になる</option>
      </select>
    </div>
  </div>

  <p v-if="loading" class="muted">読み込み中…</p>
  <p v-else-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>
  <p v-else-if="lives.length === 0" class="muted">まだ記録がありません。「+ 記録する」から追加しよう。</p>

  <div v-else style="display:grid; gap:12px;">
    <RouterLink v-for="l in lives" :key="l.id" :to="`/lives/${l.id}`" style="color:inherit;">
      <div class="card live-card">
        <div class="row" style="justify-content:space-between;">
          <strong style="font-size:17px;">{{ l.title }}</strong>
          <span class="badge" :class="l.status">{{ statusLabel[l.status] }}</span>
        </div>
        <div class="muted" style="margin-top:6px;">
          🗓 {{ l.date }} ・ 🎤 {{ l.artistName }} ・ 🏟 {{ l.venueName }}
        </div>
        <div v-if="l.setlist.length" class="muted" style="margin-top:4px; font-size:13px;">
          🎵 {{ l.setlist.length }}曲
        </div>
      </div>
    </RouterLink>
  </div>
</template>

<style scoped>
.live-card { transition: border-color .15s; }
.live-card:hover { border-color: var(--accent); }
</style>
