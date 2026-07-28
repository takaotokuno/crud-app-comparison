# ASP.NET Core Web API + React/Next.js

ASP.NET Core Web API をバックエンド、React/Next.js をフロントエンドとして CRUD アプリを実装するための作業フォルダです。

## 想定構成

```text
aspnetcore-nextjs/
├── .devcontainer/
│   └── devcontainer.json
├── backend/      # ASP.NET Core Web API プロジェクト
├── frontend/     # React/Next.js プロジェクト
├── .gitignore
└── README.md
```

## Dev Container

このフォルダには、.NET 8 SDK と Node.js 20 を利用できる Dev Container 設定を含めています。

1. VS Code でこのリポジトリを開きます。
2. `aspnetcore-nextjs` フォルダをワークスペースとして開くか、Dev Containers 拡張機能からこのフォルダの設定を選択します。
3. **Reopen in Container** を実行します。

公開ポート:

- `5000`: ASP.NET Core Web API
- `5001`: ASP.NET Core HTTPS
- `3000`: Next.js

## 初期化例

バックエンドとフロントエンドのプロジェクトは、必要に応じて次のように作成できます。

```bash
mkdir -p backend frontend

dotnet new webapi -o backend
npx create-next-app@latest frontend --ts --eslint --app --src-dir --import-alias "@/*"
```

## 実行例

Docker Compose で DB、バックエンド、フロントエンドをまとめて起動する場合:

```bash
docker compose -f docker-compose.yml -f docker-compose.app.yml up --build
```

`db` のヘルスチェックが成功してからバックエンドが起動し、バックエンドは起動時に未適用の EF Core migration を自動適用します。適用済み migration は EF Core の履歴テーブルで管理されるため、コンテナを再起動しても重複適用されません。

自動適用は Compose 環境で `Database__ApplyMigrations=true` を設定した場合だけ有効です。通常のアプリ起動では既定で無効なので、複数インスタンスを同時起動する本番環境では、デプロイ前の migration job など単一プロセスから適用してください。

バックエンド:

```bash
cd backend
dotnet run
```

ローカルで migration を手動適用する場合（SQL Server を先に起動し、接続文字列を環境に合わせて設定）:

```bash
docker compose up -d db
cd backend
dotnet tool install --global dotnet-ef --version 9.* # 初回のみ
dotnet ef database update --project AspNetNextApp.Api/AspNetNextApp.Api.csproj
```

手動適用後は通常どおり `dotnet run --project AspNetNextApp.Api/AspNetNextApp.Api.csproj` で起動できます。アプリ起動時に自動適用したい単一インスタンス環境では、`Database__ApplyMigrations=true` を設定して起動することもできます。

フロントエンド:

```bash
cd frontend
npm run dev
```

## メモ

- API の仕様、画面、CRUD 要件はリポジトリの `docs/` 配下の資料に合わせて実装します。
- ローカル専用の環境変数は `.env` または `.env.*` に配置し、必要なキーの例は `.env.example` に記載します。


## 実装メモ

### 開発用初期データ

Development 環境でバックエンドを起動すると、EF Core のマイグレーション適用後に共通要件の初期データを自動投入します。何度起動しても、メールアドレスと SKU を基準に不足分だけを追加します。

- ユーザー: `admin@example.com`、`staff@example.com`、`viewer@example.com`
- 初期パスワード: `password`（DB にはハッシュ化して保存）
- 商品・在庫: `SKU-001` から `SKU-005`

初期在庫は商品の Stock として作成し、StockTransaction には記録しません。

### データモデル実装方針

- Product は商品マスタとして扱い、Stock と StockTransaction の履歴参照元になるため物理削除しません。商品削除 API は `Products.Status = Discontinued` へ更新する論理削除として実装します。
- Stock は Product と 1:1 で保持し、Product の削除操作では削除しません。在庫数と安全在庫数は履歴整合性のため残します。
- StockTransaction は Product / Stock に紐づく在庫増減履歴として保持し、Product の削除操作では削除しません。StockTransaction が存在する Product / Stock に対して物理削除は行いません。

### 外部キーと削除制約

- 現在の EF Core 設定では Product → Stock は `DeleteBehavior.Cascade`、StockTransaction → Product は `DeleteBehavior.Restrict`、StockTransaction → Stock は `DeleteBehavior.Cascade` です。
- SQL Server で StockTransaction を作成した状態の Product を物理削除すると、StockTransaction → Product の Restrict により Product の削除は許可されません。Product → Stock → StockTransaction の Cascade 経路で履歴が消える設計にはしません。
- アプリケーション実装では Product 物理削除を実行せず、削除操作を `Status = Discontinued` への更新に統一することで、Stock / StockTransaction を履歴として保持します。

### 共通仕様との差分: 商品詳細の BFF 集約

共通仕様では、バックエンドの `GET /products/{id}` で商品、在庫、在庫取引履歴を一度に取得することを想定しています。これに対し、この構成では **Next.js の BFF（Backend for Frontend）が画面用のレスポンスを組み立てる方式**を採用しています。

商品詳細画面の BFF は、バックエンドの次の API を並行して呼び出し、結果を一つにまとめて UI へ返します。

- `GET /products/{id}`: 商品詳細
- `GET /stocks?product_id={id}&page=1&page_size=1`: 商品の在庫
- `GET /stock-transactions?product_id={id}&page=1&page_size=20`: 商品別の直近の在庫取引履歴

したがって、商品詳細画面は商品、在庫、直近の在庫取引履歴を一度の画面用リクエストで表示でき、画面要件を満たします。一方、バックエンドの `GET /products/{id}` 単体には在庫取引履歴が含まれないため、共通 API 仕様とは意図的に異なります。

#### 採用理由

- **画面固有の取得要件を BFF に閉じ込められる。** 「直近 20 件」のような表示都合を Product API の契約へ持ち込まず、ドメイン単位のバックエンド API を保てます。
- **履歴の肥大化から商品 API を守れる。** 在庫取引は増え続けるため、商品詳細レスポンスへ常に内包すると、応答サイズ、クエリ負荷、ページング仕様が商品 API に波及します。独立した一覧 API なら件数制限やページングを明示できます。
- **クライアントとバックエンド間の調整役を一か所にできる。** ブラウザが三つの API とそれぞれ通信するのではなく、認証情報の中継、並行取得、レスポンス整形、エラー変換を Next.js 側に集約できます。
- **既存 API を再利用しやすい。** 在庫一覧と在庫取引一覧は、商品詳細以外の画面や将来のクライアントからも同じ検索・ページング契約で利用できます。

この判断は「常に BFF の方が優れる」という意味ではありません。呼び出しが三つになるため、バックエンド内で一度に取得する API と比べて通信回数が増え、部分的な失敗への扱いも必要になります。また、三つの読み取りの間に在庫更新が発生すると、商品詳細画面の在庫数と最新取引が厳密に同一時点のスナップショットにならない可能性があります。現在は独立した読み取り API を並行実行し、画面固有の集約と履歴ページングを優先するため、このトレードオフを許容します。

#### 見直す条件

次のいずれかが要件になった場合は、BFF 自体を廃止するのではなく、まずバックエンドに商品詳細画面専用の集約クエリ API を追加し、BFF からその API を呼ぶ構成を検討します。

- 商品、在庫、取引履歴に厳密な同一時点の整合性が必要
- API 間通信のレイテンシーやバックエンド負荷が無視できない
- BFF 以外の複数クライアントも、まったく同じ集約レスポンスを必要とする
- 部分失敗時のフォールバックが複雑になり、BFF の責務が過大になる

現時点では BFF をやめる必要はありません。画面用 API とドメイン API の責務を分離できる利点があり、取得処理も並行化されているためです。ただし、上記の整合性または性能要件が生じた時点で、計測結果を基にバックエンド集約 API へ寄せます。
