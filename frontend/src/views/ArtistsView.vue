<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { api, type Artist } from '../api';

const artists = ref<Artist[]>([]);
const error = ref('');
const editing = ref<Partial<Artist> | null>(null);

const xUrl = (h: string) => h.startsWith('http') ? h : `https://x.com/${h.replace(/^@/, '')}`;

async function load() {
  try { artists.value = await api.artists(); }
  catch (e: any) { error.value = e.message; }
}
function startNew() { editing.value = { id: 0, name: '' }; }
function startEdit(a: Artist) { editing.value = { ...a }; }
function cancel() { editing.value = null; }

async function save() {
  if (!editing.value?.name?.trim()) { error.value = 'バンド名は必須です'; return; }
  error.value = '';
  const body = {
    name: editing.value.name,
    genre: editing.value.genre || undefined,
    officialX: editing.value.officialX || undefined,
    website: editing.value.website || undefined,
    notes: editing.value.notes || undefined,
  };
  try {
    if (editing.value.id) await api.updateArtist(editing.value.id, body);
    else await api.createArtist(body);
    editing.value = null;
    await load();
  } catch (e: any) { error.value = e.message; }
}
async function remove(a: Artist) {
  if (!confirm(`「${a.name}」を削除しますか？`)) return;
  error.value = '';
  try { await api.deleteArtist(a.id); await load(); }
  catch (e: any) { error.value = e.message; }
}
onMounted(load);
</script>

<template>
  <div class="row" style="justify-content:space-between;">
    <h1 style="margin:0;">バンド</h1>
    <button class="primary" @click="startNew">+ バンドを追加</button>
  </div>
  <p v-if="error" class="card" style="color:#ff6b8a; margin-top:12px;">{{ error }}</p>

  <div v-if="editing" class="card" style="margin:16px 0; display:grid; gap:12px;">
    <h3 style="margin:0;">{{ editing.id ? 'バンドを編集' : 'バンドを追加' }}</h3>
    <div><label>バンド名 *</label><input v-model="editing.name" /></div>
    <div class="row" style="gap:12px;">
      <div style="flex:1;"><label>ジャンル</label><input v-model="editing.genre" /></div>
      <div style="flex:1;"><label>公式X（@ハンドル or URL）</label><input v-model="editing.officialX" /></div>
    </div>
    <div><label>公式サイト</label><input v-model="editing.website" placeholder="https://" /></div>
    <div><label>メモ</label><textarea v-model="editing.notes" rows="2"></textarea></div>
    <div class="row">
      <button class="primary" @click="save">保存</button>
      <button class="ghost" @click="cancel">キャンセル</button>
    </div>
  </div>

  <div style="display:grid; gap:12px;">
    <div v-for="a in artists" :key="a.id" class="card">
      <div class="row" style="justify-content:space-between;">
        <div>
          <strong style="font-size:17px;">{{ a.name }}</strong>
          <span v-if="a.genre" class="badge" style="margin-left:8px;">{{ a.genre }}</span>
        </div>
        <div class="row" style="gap:6px;">
          <a v-if="a.officialX" :href="xUrl(a.officialX)" target="_blank">𝕏</a>
          <a v-if="a.website" :href="a.website" target="_blank">🔗</a>
          <button class="ghost" @click="startEdit(a)">編集</button>
          <button class="danger" @click="remove(a)">削除</button>
        </div>
      </div>
      <div v-if="a.notes" class="muted" style="margin-top:6px;">{{ a.notes }}</div>
    </div>
  </div>
</template>
