<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue';
import { api, type Artist } from '../api';

const artists = ref<Artist[]>([]);
const selected = ref<Set<number>>(new Set());
const title = ref(`私が観たバンド ${new Date().getFullYear()}`);
const theme = ref<keyof typeof THEMES>('neon');
const canvas = ref<HTMLCanvasElement | null>(null);
const error = ref('');

const THEMES = {
  neon:  { bg: ['#1a0033', '#3d0066'], title: '#00f0ff', band: ['#ff2d95', '#00f0ff', '#ffe600', '#9d4dff'], font: '"Arial Black", sans-serif' },
  punk:  { bg: ['#0a0a0a', '#1a1a1a'], title: '#ff2d2d', band: ['#ffffff', '#ff2d2d', '#ffd000'], font: '"Impact", "Arial Black", sans-serif' },
  emo:   { bg: ['#14141f', '#241830'], title: '#c86be0', band: ['#e0a0ff', '#7fb0ff', '#ff9ec4'], font: '"Georgia", serif' },
  chill: { bg: ['#0d1f1a', '#13332b'], title: '#6be0a0', band: ['#a0ffd0', '#ffe6a0', '#a0d8ff'], font: '"Helvetica Neue", sans-serif' },
};

function toggle(id: number) {
  const s = new Set(selected.value);
  s.has(id) ? s.delete(id) : s.add(id);
  selected.value = s;
}
function selectAll() { selected.value = new Set(artists.value.map(a => a.id)); }
function clearAll() { selected.value = new Set(); }

function roundRect(ctx: CanvasRenderingContext2D, x: number, y: number, w: number, h: number, r: number) {
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.arcTo(x + w, y, x + w, y + h, r);
  ctx.arcTo(x + w, y + h, x, y + h, r);
  ctx.arcTo(x, y + h, x, y, r);
  ctx.arcTo(x, y, x + w, y, r);
  ctx.closePath();
}

function draw() {
  const cv = canvas.value;
  if (!cv) return;
  const ctx = cv.getContext('2d')!;
  const W = cv.width;
  const t = THEMES[theme.value];
  const names = artists.value.filter(a => selected.value.has(a.id)).map(a => a.name);

  // --- レイアウト計算パス(高さを確定させてから描画し、はみ出しを防ぐ) ---
  const maxX = W - 70, lineH = 90, startY = 220;
  let x = 70, y = startY;
  const placed = names.map((name, i) => {
    const size = 30 + ((name.length * 7 + i * 13) % 34);
    ctx.font = `bold ${size}px ${t.font}`;
    const w = ctx.measureText(name).width;
    if (x + w + 40 > maxX) { x = 70; y += lineH; }
    const item = { name, size, w, x, y, color: t.band[i % t.band.length] };
    x += w + 50;
    return item;
  });
  // 確定した高さでキャンバスをリサイズ(最低700px、フッター余白140px)
  cv.height = Math.max(700, y + 140);
  const H = cv.height;

  // 背景グラデ
  const g = ctx.createLinearGradient(0, 0, W, H);
  g.addColorStop(0, t.bg[0]); g.addColorStop(1, t.bg[1]);
  ctx.fillStyle = g; ctx.fillRect(0, 0, W, H);

  // タイトル
  ctx.textAlign = 'center';
  ctx.fillStyle = t.title;
  ctx.font = `bold 64px ${t.font}`;
  ctx.shadowColor = t.title; ctx.shadowBlur = 24;
  ctx.fillText(title.value, W / 2, 110);
  ctx.shadowBlur = 0;

  // バンド名(タグクラウド風)
  ctx.textAlign = 'left';
  for (const p of placed) {
    ctx.font = `bold ${p.size}px ${t.font}`;
    ctx.fillStyle = 'rgba(255,255,255,0.06)';
    roundRect(ctx, p.x - 14, p.y - p.size, p.w + 28, p.size + 22, 12);
    ctx.fill();
    ctx.fillStyle = p.color;
    ctx.fillText(p.name, p.x, p.y);
  }

  if (names.length === 0) {
    ctx.fillStyle = 'rgba(255,255,255,0.4)';
    ctx.font = `28px ${t.font}`;
    ctx.textAlign = 'center';
    ctx.fillText('バンドを選んでください', W / 2, H / 2);
  }

  // フッター
  ctx.textAlign = 'right';
  ctx.fillStyle = 'rgba(255,255,255,0.5)';
  ctx.font = `20px ${t.font}`;
  ctx.fillText(`🎸 via LiveLog ・ ${names.length} bands`, W - 40, H - 36);
}

function download() {
  const cv = canvas.value;
  if (!cv) return;
  const a = document.createElement('a');
  a.download = `livelog-poster-${Date.now()}.png`;
  a.href = cv.toDataURL('image/png');
  a.click();
}

watch([selected, title, theme], () => nextTick(draw), { deep: true });

onMounted(async () => {
  try {
    artists.value = await api.artists();
    selectAll();
    await nextTick(); draw();
  } catch (e: any) { error.value = e.message; }
});
</script>

<template>
  <h1>🎨 バンド画像メーカー</h1>
  <p class="muted">参戦したバンドを選んで、X映えする画像を作ろう（ロゴは使わず文字で表現するので著作権フリー）</p>
  <p v-if="error" class="card" style="color:#ff6b8a;">{{ error }}</p>

  <div class="card" style="display:grid; gap:14px;">
    <div>
      <label>タイトル</label>
      <input v-model="title" />
    </div>
    <div>
      <label>テーマ</label>
      <div class="row" style="gap:8px;">
        <button v-for="(_, key) in THEMES" :key="key"
          :class="theme === key ? 'primary' : 'ghost'" @click="theme = key as any">{{ key }}</button>
      </div>
    </div>
    <div>
      <div class="row" style="justify-content:space-between;">
        <label style="margin:0;">バンドを選択（{{ selected.size }}）</label>
        <div class="row" style="gap:6px;">
          <button class="ghost" @click="selectAll">全選択</button>
          <button class="ghost" @click="clearAll">解除</button>
        </div>
      </div>
      <div class="row" style="gap:6px; margin-top:8px;">
        <button v-for="a in artists" :key="a.id"
          :class="selected.has(a.id) ? 'primary' : 'ghost'" @click="toggle(a.id)">{{ a.name }}</button>
        <span v-if="artists.length === 0" class="muted">バンドが未登録です</span>
      </div>
    </div>
  </div>

  <div class="card" style="margin-top:16px; text-align:center;">
    <canvas ref="canvas" width="1000" height="700" class="poster"></canvas>
    <div style="margin-top:12px;">
      <button class="primary" @click="download">⬇ PNGをダウンロード</button>
    </div>
  </div>
</template>

<style scoped>
.poster { width:100%; max-width:1000px; border-radius:12px; border:1px solid var(--border); }
</style>
