// LiveLog API クライアント
const BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5219';

export type LiveStatus = 'Interested' | 'Applied' | 'Attended';

export interface Artist {
  id: number; name: string; genre?: string; notes?: string;
  officialX?: string; website?: string;
}
export interface Venue {
  id: number; name: string; city?: string; capacity?: number;
  drinkFee?: number; acceptsEMoney?: boolean; officialX?: string; liveCount: number;
}
export interface Song { id: number; title: string; artistId: number; artistName?: string; }
export interface SetlistItem { songId: number; title: string; order: number; isEncore: boolean; }
export interface Live {
  id: number; title: string; date: string;
  artistId: number; artistName: string;
  venueId: number; venueName: string;
  status: LiveStatus; notes?: string; setlist: SetlistItem[];
}
export interface SongPlay { liveId: number; liveTitle: string; date: string; venueName: string; order: number; isEncore: boolean; }
export interface SongSummary { songId: number; title: string; artistName: string; playCount: number; plays: SongPlay[]; }

export interface NameCount { name: string; count: number; }
export interface YearCount { year: number; count: number; }
export interface Stats {
  totalAttended: number;
  thisYearAttended: number;
  upcomingCount: number;
  totalSongsPlayed: number;
  uniqueSongsHeard: number;
  byYear: YearCount[];
  topVenues: NameCount[];
  topArtists: NameCount[];
}

export interface LiveInput {
  title: string; date: string; artistId: number; venueId: number;
  status: LiveStatus; notes?: string;
  setlist: { songId: number; order: number; isEncore: boolean }[];
}

async function http<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  });
  const text = await res.text();
  const body = text ? JSON.parse(text) : undefined;
  if (!res.ok) {
    // バックエンドが返す { error: "..." } を優先して表示
    throw new Error(body?.error ?? `${res.status} ${res.statusText}`);
  }
  return body as T;
}

export const api = {
  // Lives
  lives: (params: { status?: LiveStatus; venueId?: number; artistId?: number } = {}) => {
    const q = new URLSearchParams();
    if (params.status) q.set('status', params.status);
    if (params.venueId) q.set('venueId', String(params.venueId));
    if (params.artistId) q.set('artistId', String(params.artistId));
    const s = q.toString();
    return http<Live[]>(`/api/lives${s ? `?${s}` : ''}`);
  },
  live: (id: number) => http<Live>(`/api/lives/${id}`),
  createLive: (body: LiveInput) => http<Live>('/api/lives', { method: 'POST', body: JSON.stringify(body) }),
  updateLive: (id: number, body: LiveInput) => http<Live>(`/api/lives/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteLive: (id: number) => http<void>(`/api/lives/${id}`, { method: 'DELETE' }),

  // Artists
  artists: () => http<Artist[]>('/api/artists'),
  createArtist: (body: Partial<Artist>) => http<Artist>('/api/artists', { method: 'POST', body: JSON.stringify(body) }),
  updateArtist: (id: number, body: Partial<Artist>) => http<Artist>(`/api/artists/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteArtist: (id: number) => http<void>(`/api/artists/${id}`, { method: 'DELETE' }),

  // Venues
  venues: () => http<Venue[]>('/api/venues'),
  createVenue: (body: Partial<Venue>) => http<Venue>('/api/venues', { method: 'POST', body: JSON.stringify(body) }),
  updateVenue: (id: number, body: Partial<Venue>) => http<Venue>(`/api/venues/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteVenue: (id: number) => http<void>(`/api/venues/${id}`, { method: 'DELETE' }),

  // Songs
  songs: (q?: string, artistId?: number) => {
    const p = new URLSearchParams();
    if (q) p.set('q', q);
    if (artistId) p.set('artistId', String(artistId));
    const s = p.toString();
    return http<Song[]>(`/api/songs${s ? `?${s}` : ''}`);
  },
  createSong: (body: { title: string; artistId: number }) =>
    http<Song>('/api/songs', { method: 'POST', body: JSON.stringify(body) }),
  songHistory: (id: number, from?: string, to?: string) => {
    const p = new URLSearchParams();
    if (from) p.set('from', from);
    if (to) p.set('to', to);
    const s = p.toString();
    return http<SongSummary>(`/api/songs/${id}/history${s ? `?${s}` : ''}`);
  },

  // Stats
  stats: () => http<Stats>('/api/stats/summary'),
};

export const statusLabel: Record<LiveStatus, string> = {
  Interested: '気になる',
  Applied: '応募済み',
  Attended: '参戦済み',
};
