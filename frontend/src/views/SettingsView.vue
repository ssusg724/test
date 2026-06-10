<script setup lang="ts">
import { ref } from 'vue';
import { api } from '../api';
import { theme, toggleTheme } from '../theme';

const msg = ref('');
const error = ref('');
const importing = ref(false);

async function doExport() {
  error.value = ''; msg.value = '';
  try {
    const data = await api.exportData();
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = `livelog-backup-${new Date().toISOString().slice(0, 10)}.json`;
    a.click();
    URL.revokeObjectURL(a.href);
    msg.value = 'バックアップをダウンロードしました';
  } catch (e: any) { error.value = `エクスポート失敗: ${e.message}`; }
}

async function onFile(ev: Event) {
  error.value = ''; msg.value = '';
  const file = (ev.target as HTMLInputElement).files?.[0];
  if (!file) return;
  if (!confirm('インポートすると現在のデータは全て置き換えられます。続けますか？')) {
    (ev.target as HTMLInputElement).value = '';
    return;
  }
  importing.value = true;
  try {
    const bundle = JSON.parse(await file.text());
    const r = await api.importData(bundle);
    msg.value = `インポート完了: バンド${r.artists} / 会場${r.venues} / 曲${r.songs} / ライブ${r.lives}`;
  } catch (e: any) {
    error.value = `インポート失敗: ${e.message}`;
  } finally {
    importing.value = false;
    (ev.target as HTMLInputElement).value = '';
  }
}
</script>

<template>
  <h1>⚙️ 設定</h1>
  <p v-if="msg" class="card" style="color:#6be09a;">{{ msg }}</p>
  <p v-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>

  <div class="card" style="margin-bottom:16px;">
    <h3 style="margin-top:0;">テーマ</h3>
    <div class="row" style="justify-content:space-between;">
      <span class="muted">現在: {{ theme === 'dark' ? 'ダーク' : 'ライト' }}</span>
      <button class="ghost" @click="toggleTheme">{{ theme === 'dark' ? '☀️ ライトにする' : '🌙 ダークにする' }}</button>
    </div>
  </div>

  <div class="card" style="margin-bottom:16px;">
    <h3 style="margin-top:0;">バックアップ</h3>
    <p class="muted">全データをJSONファイルに書き出します。別環境への移行や保存に。</p>
    <button class="primary" @click="doExport">⬇ エクスポート</button>
  </div>

  <div class="card">
    <h3 style="margin-top:0;">復元（インポート）</h3>
    <p class="muted" style="color:#ff9a6b;">⚠️ 現在のデータは全て置き換えられます。</p>
    <label class="filebtn">
      <input type="file" accept="application/json" @change="onFile" :disabled="importing" hidden />
      {{ importing ? '取り込み中…' : '📁 JSONを選んでインポート' }}
    </label>
  </div>
</template>

<style scoped>
.filebtn {
  display:inline-block; cursor:pointer; padding:8px 14px; border-radius:8px;
  background:var(--surface-2); border:1px solid var(--border); color:var(--text); margin:0;
}
.filebtn:hover { filter:brightness(1.2); }
</style>
