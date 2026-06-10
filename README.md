# 🎸 LiveLog — ライブ記録アプリ

行ったロックバンドのライブを記録し、**会場ごと・曲ごと**に振り返れるアプリ。
「あの曲、前回いつ演奏された？」をセトリから検索でき、参戦統計のダッシュボードや、
X映えする「私が観たバンド」画像メーカーも備えています。

## ✨ 機能

- **ライブ記録** … バンド・会場・日付・セットリスト（演奏順／アンコール）を登録
- **ステータス管理** … `参戦済み / 応募済み / 気になる`（今後行きたいライブの管理にも）
- **会場ごとのまとめ** … キャパ・🥤ドリンク代・💳電子マネー可否・公式X を一覧
- **曲ごとのまとめ／検索** … その曲を生で何回聴いたか、いつ・どの会場でかの履歴。期間（from/to）で絞り込み
- **参戦ダッシュボード** … 通算／今年／年別の参戦数、よく行く会場・バンド、生で聴いた曲数
- **🎨 バンド画像メーカー** … 参戦したバンドを選んでCanvasで画像生成 → PNG出力（ロゴを使わず文字で表現するので著作権フリー）
- バンド／会場の**公式Xリンク**表示

## 🧱 技術スタック

| | |
|---|---|
| バックエンド | ASP.NET Core 8 Web API + **Entity Framework Core 8**（SQLite） |
| フロントエンド | **Vue 3** + Vite + TypeScript + Vue Router |
| DB | SQLite（`backend/livelog.db` に自動生成・初回サンプルデータ投入） |

## 🚀 動かし方

前提: .NET SDK 8 / Node.js 20+

### 1. バックエンド（API）

```bash
cd backend
dotnet run
```

→ `http://localhost:5219` で起動（Swagger: `http://localhost:5219/swagger`）。
初回起動時に SQLite DB を作成し、サンプルデータを投入します。

### 2. フロントエンド

別ターミナルで:

```bash
cd frontend
npm install
npm run dev
```

→ `http://localhost:5173` をブラウザで開く。

> API のURLを変えたい場合は `frontend` に `.env` を作り
> `VITE_API_BASE=http://localhost:5219` を設定。

## 📁 構成

```
backend/
  Controllers/   ライブ/会場/曲/バンド/統計 の各API
  Models/        エンティティ + DTO
  Data/          DbContext + サンプルデータ
frontend/
  src/views/     各画面(ダッシュボード/一覧/詳細/登録/会場/曲検索/画像メーカー)
  src/api.ts     APIクライアント + 型定義
```

## 🗺 API 概要

| メソッド | パス | 説明 |
|---|---|---|
| GET | `/api/lives?status=&venueId=&artistId=` | ライブ一覧（絞り込み可） |
| POST/PUT/DELETE | `/api/lives/{id}` | ライブの作成・更新・削除（セトリ込み） |
| GET | `/api/venues` | 会場一覧（公演数つき） |
| GET | `/api/songs?q=` | 曲名検索 |
| GET | `/api/songs/{id}/history?from=&to=` | 曲の演奏履歴・期間集計 |
| GET | `/api/stats/summary` | 参戦統計サマリー |

## 📝 メモ

- DB は開発用に `EnsureCreated()` で生成しています。モデル（カラム）を変更したときは
  `backend/livelog.db*` を削除して作り直してください（本番運用時は EF Migrations へ移行を推奨）。
- 不正な参照（存在しないバンド/会場/曲）には 400、参照されている会場/バンドの削除には 409 を返します。
