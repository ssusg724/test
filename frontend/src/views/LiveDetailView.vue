<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { api, statusLabel, type Live, type Artist, type Venue } from '../api';

const props = defineProps<{ id: string }>();
const router = useRouter();
const live = ref<Live | null>(null);
const artist = ref<Artist | null>(null);
const venue = ref<Venue | null>(null);
const error = ref('');

const xUrl = (h: string) => h.startsWith('http') ? h : `https://x.com/${h.replace(/^@/, '')}`;

const main = computed(() => live.value?.setlist.filter(s => !s.isEncore) ?? []);
const encore = computed(() => live.value?.setlist.filter(s => s.isEncore) ?? []);
const copied = ref(false);

// X投稿用のセトリテキストを組み立てる
const shareText = computed(() => {
  const l = live.value;
  if (!l) return '';
  const lines = [`🎸 ${l.artistName} @ ${l.venueName} (${l.date})`, ''];
  main.value.forEach((s, i) => lines.push(`${i + 1}. ${s.title}`));
  if (encore.value.length) {
    lines.push('', '＜ENCORE＞');
    encore.value.forEach((s, i) => lines.push(`E${i + 1}. ${s.title}`));
  }
  lines.push('', '#LiveLog');
  return lines.join('\n');
});

function shareToX() {
  const url = `https://x.com/intent/tweet?text=${encodeURIComponent(shareText.value)}`;
  window.open(url, '_blank');
}
async function copySetlist() {
  try {
    await navigator.clipboard.writeText(shareText.value);
    copied.value = true;
    setTimeout(() => (copied.value = false), 1800);
  } catch { /* クリップボード非対応環境は無視 */ }
}

async function load() {
  try {
    live.value = await api.live(Number(props.id));
    const [artists, venues] = await Promise.all([api.artists(), api.venues()]);
    artist.value = artists.find(a => a.id === live.value!.artistId) ?? null;
    venue.value = venues.find(v => v.id === live.value!.venueId) ?? null;
  } catch (e: any) { error.value = e.message; }
}
async function remove() {
  if (!confirm('この記録を削除しますか？')) return;
  await api.deleteLive(Number(props.id));
  router.push('/lives');
}
onMounted(load);
</script>

<template>
  <p v-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>
  <template v-else-if="live">
    <RouterLink to="/lives" class="muted">← 一覧へ</RouterLink>
    <div class="row" style="justify-content:space-between; margin:12px 0;">
      <h1 style="margin:0;">{{ live.title }}</h1>
      <span class="badge" :class="live.status">{{ statusLabel[live.status] }}</span>
    </div>

    <div class="card" style="display:grid; gap:8px;">
      <div>🗓 <strong>{{ live.date }}</strong></div>
      <div>
        🎤 {{ live.artistName }}
        <a v-if="artist?.officialX" :href="xUrl(artist.officialX)" target="_blank" style="margin-left:8px;">𝕏 公式</a>
      </div>
      <div>
        🏟 {{ live.venueName }}
        <span v-if="venue?.drinkFee != null" class="muted" style="margin-left:8px;">🥤¥{{ venue.drinkFee }}</span>
        <span v-if="venue?.acceptsEMoney === false" class="badge" style="margin-left:6px; color:#ff6b8a;">現金のみ</span>
        <span v-else-if="venue?.acceptsEMoney === true" class="badge" style="margin-left:6px; color:#6be09a;">💳OK</span>
      </div>
      <div v-if="live.notes" class="muted">📝 {{ live.notes }}</div>
    </div>

    <div class="row" style="justify-content:space-between; margin-top:24px;">
      <h2 style="margin:0;">セットリスト</h2>
      <div v-if="live.setlist.length" class="row" style="gap:6px;">
        <button class="ghost" @click="shareToX">𝕏 シェア</button>
        <button class="ghost" @click="copySetlist">{{ copied ? '✓ コピーした' : '📋 コピー' }}</button>
      </div>
    </div>
    <div v-if="live.setlist.length === 0" class="muted" style="margin-top:8px;">セトリ未登録</div>
    <div v-else class="card" style="margin-top:8px;">
      <ol class="setlist">
        <li v-for="s in main" :key="s.order">{{ s.title }}</li>
      </ol>
      <template v-if="encore.length">
        <div class="muted" style="margin:10px 0 4px; font-weight:600;">— ENCORE —</div>
        <ol class="setlist encore">
          <li v-for="s in encore" :key="s.order">{{ s.title }}</li>
        </ol>
      </template>
    </div>

    <div class="row" style="margin-top:24px;">
      <RouterLink :to="`/lives/${live.id}/edit`"><button class="ghost">編集</button></RouterLink>
      <button class="danger" @click="remove">削除</button>
    </div>
  </template>
</template>

<style scoped>
.setlist { margin: 0; padding-left: 28px; line-height: 2; }
.setlist.encore { color: var(--accent-2); }
</style>
