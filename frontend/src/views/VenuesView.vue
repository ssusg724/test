<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { api, type Venue, type Live } from '../api';

const venues = ref<Venue[]>([]);
const expanded = ref<number | null>(null);
const livesByVenue = ref<Record<number, Live[]>>({});
const error = ref('');

const xUrl = (h: string) => h.startsWith('http') ? h : `https://x.com/${h.replace(/^@/, '')}`;

async function load() {
  try { venues.value = await api.venues(); }
  catch (e: any) { error.value = e.message; }
}
async function toggle(v: Venue) {
  if (expanded.value === v.id) { expanded.value = null; return; }
  expanded.value = v.id;
  if (!livesByVenue.value[v.id]) {
    livesByVenue.value[v.id] = await api.lives({ venueId: v.id });
  }
}
onMounted(load);
</script>

<template>
  <h1>会場</h1>
  <p v-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>

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
      </div>
    </div>
  </div>
</template>
