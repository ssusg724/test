<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { api, type Artist, type Venue, type Song, type LiveStatus } from '../api';

const props = defineProps<{ id?: string }>();
const router = useRouter();
const isEdit = !!props.id;

const artists = ref<Artist[]>([]);
const venues = ref<Venue[]>([]);
const songs = ref<Song[]>([]);
const error = ref('');
const saving = ref(false);

// フォーム状態
const title = ref('');
const date = ref(new Date().toISOString().slice(0, 10));
const artistId = ref<number | null>(null);
const venueId = ref<number | null>(null);
const status = ref<LiveStatus>('Attended');
const notes = ref('');
let uidSeq = 0;
const setlist = ref<{ uid: number; songId: number; title: string; isEncore: boolean }[]>([]);

const newSongTitle = ref('');
let initializing = false;

async function loadRefs() {
  [artists.value, venues.value] = await Promise.all([api.artists(), api.venues()]);
}
async function loadSongs() {
  songs.value = artistId.value ? await api.songs(undefined, artistId.value) : [];
}
// バンドを変えたら曲候補を切り替え、ユーザー操作時はセトリもクリア(別バンドの曲が混ざらないように)
watch(artistId, async () => {
  await loadSongs();
  if (!initializing) setlist.value = [];
});

async function loadExisting() {
  if (!props.id) return;
  initializing = true;
  try {
    const l = await api.live(Number(props.id));
    title.value = l.title; date.value = l.date;
    artistId.value = l.artistId; venueId.value = l.venueId;
    status.value = l.status; notes.value = l.notes ?? '';
    await loadSongs();
    setlist.value = l.setlist.map(s => ({ uid: uidSeq++, songId: s.songId, title: s.title, isEncore: s.isEncore }));
  } finally {
    initializing = false;
  }
}

onMounted(async () => {
  try { await loadRefs(); await loadExisting(); }
  catch (e: any) { error.value = e.message; }
});

async function quickAddArtist() {
  const name = prompt('バンド名'); if (!name) return;
  const a = await api.createArtist({ name });
  artists.value.push(a); artistId.value = a.id;
}
async function quickAddVenue() {
  const name = prompt('会場名'); if (!name) return;
  const v = await api.createVenue({ name });
  venues.value.push(v); venueId.value = v.id;
}

function addSong(s: Song) {
  setlist.value.push({ uid: uidSeq++, songId: s.id, title: s.title, isEncore: false });
}
async function addNewSong() {
  if (!newSongTitle.value.trim() || !artistId.value) return;
  const s = await api.createSong({ title: newSongTitle.value.trim(), artistId: artistId.value });
  songs.value.push(s);
  addSong(s);
  newSongTitle.value = '';
}
function move(i: number, dir: -1 | 1) {
  const j = i + dir;
  if (j < 0 || j >= setlist.value.length) return;
  const arr = setlist.value;
  [arr[i], arr[j]] = [arr[j], arr[i]];
}
function removeAt(i: number) { setlist.value.splice(i, 1); }

async function save() {
  error.value = '';
  if (!title.value || !artistId.value || !venueId.value) {
    error.value = 'タイトル・バンド・会場は必須です';
    return;
  }
  saving.value = true;
  try {
    const body = {
      title: title.value, date: date.value,
      artistId: artistId.value, venueId: venueId.value,
      status: status.value, notes: notes.value || undefined,
      setlist: setlist.value.map((s, i) => ({ songId: s.songId, order: i + 1, isEncore: s.isEncore })),
    };
    const saved = isEdit
      ? await api.updateLive(Number(props.id), body)
      : await api.createLive(body);
    router.push(`/lives/${saved.id}`);
  } catch (e: any) {
    error.value = e.message;
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <h1>{{ isEdit ? 'ライブを編集' : 'ライブを記録' }}</h1>
  <p v-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>

  <div class="card" style="display:grid; gap:14px;">
    <div>
      <label>タイトル</label>
      <input v-model="title" placeholder="例: ◯◯ TOUR 2025" />
    </div>
    <div class="row" style="gap:14px;">
      <div style="flex:1;">
        <label>日付</label>
        <input type="date" v-model="date" />
      </div>
      <div style="flex:1;">
        <label>ステータス</label>
        <select v-model="status">
          <option value="Attended">参戦済み</option>
          <option value="Applied">応募済み</option>
          <option value="Interested">気になる</option>
        </select>
      </div>
    </div>

    <div class="row" style="gap:14px; align-items:flex-end;">
      <div style="flex:1;">
        <label>バンド</label>
        <select v-model="artistId">
          <option :value="null" disabled>選択…</option>
          <option v-for="a in artists" :key="a.id" :value="a.id">{{ a.name }}</option>
        </select>
      </div>
      <button class="ghost" @click="quickAddArtist">+ 新規</button>
    </div>

    <div class="row" style="gap:14px; align-items:flex-end;">
      <div style="flex:1;">
        <label>会場</label>
        <select v-model="venueId">
          <option :value="null" disabled>選択…</option>
          <option v-for="v in venues" :key="v.id" :value="v.id">{{ v.name }}</option>
        </select>
      </div>
      <button class="ghost" @click="quickAddVenue">+ 新規</button>
    </div>

    <div>
      <label>メモ</label>
      <textarea v-model="notes" rows="2" placeholder="感想など"></textarea>
    </div>
  </div>

  <!-- セトリビルダー -->
  <h2 style="margin-top:24px;">セットリスト</h2>
  <div v-if="!artistId" class="muted">先にバンドを選ぶと曲を追加できます</div>
  <template v-else>
    <div class="card" style="margin-bottom:12px;">
      <label>曲を追加（{{ '既存の曲をクリック' }}）</label>
      <div class="row" style="gap:8px;">
        <button v-for="s in songs" :key="s.id" class="ghost" @click="addSong(s)">+ {{ s.title }}</button>
        <span v-if="songs.length === 0" class="muted">登録済みの曲なし</span>
      </div>
      <div class="row" style="margin-top:12px; gap:8px;">
        <input v-model="newSongTitle" placeholder="新しい曲名" @keyup.enter="addNewSong" style="flex:1;" />
        <button class="ghost" @click="addNewSong">曲を新規追加</button>
      </div>
    </div>

    <div v-if="setlist.length" class="card">
      <div v-for="(s, i) in setlist" :key="s.uid" class="setrow">
        <span class="num">{{ i + 1 }}</span>
        <span style="flex:1;">{{ s.title }}</span>
        <label class="enc"><input type="checkbox" v-model="s.isEncore" style="width:auto;" /> アンコール</label>
        <button class="ghost" @click="move(i, -1)">↑</button>
        <button class="ghost" @click="move(i, 1)">↓</button>
        <button class="danger" @click="removeAt(i)">×</button>
      </div>
    </div>
  </template>

  <div class="row" style="margin-top:24px;">
    <button class="primary" :disabled="saving" @click="save">{{ saving ? '保存中…' : '保存' }}</button>
    <button class="ghost" @click="router.back()">キャンセル</button>
  </div>
</template>

<style scoped>
.setrow { display:flex; align-items:center; gap:8px; padding:6px 0; border-bottom:1px solid var(--border); }
.setrow:last-child { border-bottom:none; }
.num { width:24px; text-align:right; color:var(--muted); }
.enc { display:flex; align-items:center; gap:4px; font-size:13px; margin:0; white-space:nowrap; }
</style>
