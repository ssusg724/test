<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { api, type Venue, type Live } from '../api';

const venues = ref<Venue[]>([]);
const expanded = ref<number | null>(null);
const livesByVenue = ref<Record<number, Live[]>>({});
const error = ref('');

// 編集フォーム状態（id=0 のとき新規）
const editing = ref<Partial<Venue> | null>(null);

const xUrl = (h: string) => h.startsWith('http') ? h : `https://x.com/${h.replace(/^@/, '')}`;

async function load() {
  try { venues.value = await api.venues(); }
  catch (e: any) { error.value = e.message; }
}
async function toggle(v: Venue) {
  if (expanded.value === v.id) { expanded.value = null; return; }
  expanded.value = v.id;
  // 常に取り直して最新の公演リストを表示(他画面での追加/編集を反映)
  livesByVenue.value[v.id] = await api.lives({ venueId: v.id });
}

function startNew() { editing.value = { id: 0, name: '', acceptsEMoney: undefined }; }
function startEdit(v: Venue) { editing.value = { ...v }; }
function cancel() { editing.value = null; }

async function save() {
  if (!editing.value?.name?.trim()) { error.value = '会場名は必須です'; return; }
  error.value = '';
  const num = (v: unknown) => (typeof v === 'number' && Number.isFinite(v) ? v : undefined);
  const body = {
    name: editing.value.name,
    city: editing.value.city || undefined,
    capacity: num(editing.value.capacity),
    drinkFee: num(editing.value.drinkFee),
    acceptsEMoney: editing.value.acceptsEMoney,
    officialX: editing.value.officialX || undefined,
  };
  try {
    if (editing.value.id) await api.updateVenue(editing.value.id, body);
    else await api.createVenue(body);
    editing.value = null;
    await load();
  } catch (e: any) { error.value = e.message; }
}

async function remove(v: Venue) {
  if (!confirm(`「${v.name}」を削除しますか？`)) return;
  error.value = '';
  try { await api.deleteVenue(v.id); await load(); }
  catch (e: any) { error.value = e.message; }
}

onMounted(load);
</script>

<template>
  <div class="row" style="justify-content:space-between;">
    <h1 style="margin:0;">会場</h1>
    <button class="primary" @click="startNew">+ 会場を追加</button>
  </div>
  <p v-if="error" class="card" style="color:#ff6b8a; margin-top:12px;">{{ error }}</p>

  <!-- 編集/新規フォーム -->
  <div v-if="editing" class="card" style="margin:16px 0; display:grid; gap:12px;">
    <h3 style="margin:0;">{{ editing.id ? '会場を編集' : '会場を追加' }}</h3>
    <div><label>会場名 *</label><input v-model="editing.name" /></div>
    <div class="row" style="gap:12px;">
      <div style="flex:1;"><label>エリア/都市</label><input v-model="editing.city" /></div>
      <div style="flex:1;"><label>キャパ</label><input type="number" v-model.number="editing.capacity" /></div>
    </div>
    <div class="row" style="gap:12px; align-items:flex-end;">
      <div style="flex:1;"><label>ドリンク代(円)</label><input type="number" v-model.number="editing.drinkFee" /></div>
      <div style="flex:1;">
        <label>キャッシュレス</label>
        <select v-model="editing.acceptsEMoney">
          <option :value="undefined">不明</option>
          <option :value="true">対応</option>
          <option :value="false">現金のみ</option>
        </select>
      </div>
    </div>
    <div><label>公式X（@ハンドル or URL）</label><input v-model="editing.officialX" /></div>
    <div class="row">
      <button class="primary" @click="save">保存</button>
      <button class="ghost" @click="cancel">キャンセル</button>
    </div>
  </div>

  <div style="display:grid; gap:12px;">
    <div v-for="v in venues" :key="v.id" class="card">
      <div class="row" style="justify-content:space-between; cursor:pointer;" @click="toggle(v)">
        <div>
          <strong style="font-size:17px;">{{ v.name }}</strong>
          <span v-if="v.city" class="muted"> ・ {{ v.city }}</span>
        </div>
        <span class="badge">{{ v.liveCount }} 公演</span>
      </div>

      <div class="row" style="margin-top:10px; gap:8px;">
        <span class="badge" v-if="v.capacity">キャパ {{ v.capacity.toLocaleString() }}</span>
        <span class="badge" v-if="v.drinkFee != null">🥤 ドリンク代 ¥{{ v.drinkFee }}</span>
        <span class="badge" v-if="v.acceptsEMoney === true" style="color:#6be09a;">💳 電子マネーOK</span>
        <span class="badge" v-else-if="v.acceptsEMoney === false" style="color:#ff6b8a;">現金のみ</span>
        <a v-if="v.officialX" :href="xUrl(v.officialX)" target="_blank" @click.stop>𝕏 公式</a>
      </div>

      <div v-if="expanded === v.id" style="margin-top:12px; border-top:1px solid var(--border); padding-top:12px;">
        <p v-if="(livesByVenue[v.id]?.length ?? 0) === 0" class="muted">この会場の記録なし</p>
        <RouterLink v-for="l in livesByVenue[v.id]" :key="l.id" :to="`/lives/${l.id}`"
          class="muted" style="display:block; padding:4px 0;">
          🗓 {{ l.date }} ・ {{ l.title }}（{{ l.artistName }}）
        </RouterLink>
        <div class="row" style="margin-top:10px;">
          <button class="ghost" @click.stop="startEdit(v)">編集</button>
          <button class="danger" @click.stop="remove(v)">削除</button>
        </div>
      </div>
    </div>
  </div>
</template>
