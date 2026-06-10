<script setup lang="ts">
import { ref } from 'vue';
import { api, type Song, type SongSummary } from '../api';

const q = ref('');
const results = ref<Song[]>([]);
const selected = ref<SongSummary | null>(null);
const from = ref('');
const to = ref('');
const searched = ref(false);
const error = ref('');

async function search() {
  error.value = '';
  searched.value = true;
  selected.value = null;
  try { results.value = await api.songs(q.value.trim() || undefined); }
  catch (e: any) { error.value = `検索失敗: ${e.message}`; }
}
async function showHistory(s: Song) {
  error.value = '';
  try { selected.value = await api.songHistory(s.id, from.value || undefined, to.value || undefined); }
  catch (e: any) { error.value = `履歴の取得に失敗: ${e.message}`; }
}
async function refilter() {
  if (!selected.value) return;
  error.value = '';
  try { selected.value = await api.songHistory(selected.value.songId, from.value || undefined, to.value || undefined); }
  catch (e: any) { error.value = `絞り込みに失敗: ${e.message}`; }
}
</script>

<template>
  <h1>曲を探す</h1>
  <p class="muted">「あの曲、前回いつ演奏された？」を曲名から調べられます。</p>
  <p v-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>

  <div class="card" style="margin-bottom:16px;">
    <div class="row" style="gap:8px;">
      <input v-model="q" placeholder="曲名で検索（空欄で全曲）" @keyup.enter="search" style="flex:1;" />
      <button class="primary" @click="search">検索</button>
    </div>
  </div>

  <div v-if="searched && results.length === 0" class="muted">該当する曲がありません</div>

  <div v-if="!selected" style="display:grid; gap:8px;">
    <div v-for="s in results" :key="s.id" class="card row" style="justify-content:space-between;">
      <div>
        <strong>{{ s.title }}</strong>
        <span class="muted"> ／ {{ s.artistName }}</span>
      </div>
      <button class="ghost" @click="showHistory(s)">演奏履歴 →</button>
    </div>
  </div>

  <!-- 曲ごとのまとめ -->
  <div v-else>
    <button class="ghost" @click="selected = null">← 検索結果へ</button>
    <div class="card" style="margin:12px 0;">
      <h2 style="margin:0 0 4px;">{{ selected.title }}</h2>
      <div class="muted">{{ selected.artistName }}</div>
      <div style="margin-top:12px; font-size:28px; font-weight:800; color:var(--accent-2);">
        {{ selected.playCount }} <span style="font-size:14px; color:var(--muted);">回 生で聴いた</span>
      </div>

      <div class="row" style="margin-top:12px; gap:8px; align-items:flex-end;">
        <div><label>期間 From</label><input type="date" v-model="from" @change="refilter" /></div>
        <div><label>To</label><input type="date" v-model="to" @change="refilter" /></div>
        <button class="ghost" @click="from=''; to=''; refilter()">クリア</button>
      </div>
    </div>

    <div class="card">
      <p v-if="selected.plays.length === 0" class="muted">この期間の演奏記録なし</p>
      <RouterLink v-for="p in selected.plays" :key="p.liveId + '-' + p.order" :to="`/lives/${p.liveId}`"
        class="playrow">
        <span>🗓 {{ p.date }}</span>
        <span style="flex:1;">{{ p.liveTitle }} ・ {{ p.venueName }}</span>
        <span class="badge" v-if="p.isEncore" style="color:var(--accent-2);">アンコール</span>
      </RouterLink>
    </div>
  </div>
</template>

<style scoped>
.playrow {
  display:flex; gap:12px; align-items:center; color:inherit;
  padding:8px 0; border-bottom:1px solid var(--border);
}
.playrow:last-child { border-bottom:none; }
.playrow:hover { color:var(--accent-2); }
</style>
