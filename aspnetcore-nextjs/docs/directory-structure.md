# aspnetcore-nextjs ディレクトリ構成案

ASP.NET Core Web API と Next.js を分離した構成で、共通仕様の「商品在庫管理CRUD」を実装・比較しやすくするためのディレクトリ構成案です。

## 方針

- バックエンドとフロントエンドは `backend/` と `frontend/` に明確に分離する。
- API 契約、認証方式、環境変数、起動手順など、両者にまたがる情報は `docs/` に集約する。
- ASP.NET Core 側はレイヤーを分けすぎず、CRUD アプリとして見通しのよい `Api / Application / Domain / Infrastructure` の4層を基本にする。
- Next.js 側は App Router を前提に、ルート、UI 部品、API クライアント、型定義を分ける。
- テストは各プロジェクト配下に置き、バックエンド・フロントエンド単体で実行できるようにする。

## 推奨ツリー

```text
aspnetcore-nextjs/
├── .devcontainer/
│   └── devcontainer.json
├── backend/
│   ├── ProductInventory.Api/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── ProductsController.cs
│   │   │   ├── StocksController.cs
│   │   │   └── StockTransactionsController.cs
│   │   ├── Contracts/
│   │   │   ├── Auth/
│   │   │   ├── Products/
│   │   │   ├── Stocks/
│   │   │   └── StockTransactions/
│   │   ├── Extensions/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   ├── appsettings.Development.json
│   │   └── appsettings.json
│   ├── ProductInventory.Application/
│   │   ├── Auth/
│   │   ├── Products/
│   │   ├── Stocks/
│   │   ├── StockTransactions/
│   │   ├── Common/
│   │   └── Validation/
│   ├── ProductInventory.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Product.cs
│   │   │   ├── Stock.cs
│   │   │   └── StockTransaction.cs
│   │   ├── Enums/
│   │   └── Errors/
│   ├── ProductInventory.Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── Seed/
│   │   ├── Authentication/
│   │   └── Repositories/
│   ├── ProductInventory.Tests/
│   │   ├── Products/
│   │   ├── Stocks/
│   │   └── StockTransactions/
│   └── ProductInventory.sln
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── (auth)/
│   │   │   │   └── login/
│   │   │   ├── (protected)/
│   │   │   │   ├── layout.tsx
│   │   │   │   ├── products/
│   │   │   │   │   ├── page.tsx
│   │   │   │   │   ├── new/
│   │   │   │   │   └── [productId]/
│   │   │   │   ├── stocks/
│   │   │   │   └── stock-transactions/
│   │   │   ├── layout.tsx
│   │   │   └── page.tsx
│   │   ├── components/
│   │   │   ├── auth/
│   │   │   ├── products/
│   │   │   ├── stocks/
│   │   │   ├── stock-transactions/
│   │   │   └── ui/
│   │   ├── features/
│   │   │   ├── auth/
│   │   │   ├── products/
│   │   │   ├── stocks/
│   │   │   └── stock-transactions/
│   │   ├── lib/
│   │   │   ├── api-client.ts
│   │   │   ├── auth.ts
│   │   │   └── format.ts
│   │   ├── schemas/
│   │   └── types/
│   ├── tests/
│   │   ├── unit/
│   │   └── e2e/
│   ├── .env.example
│   ├── next.config.ts
│   ├── package.json
│   └── tsconfig.json
├── docs/
│   ├── directory-structure.md
│   ├── implementation-note.md
│   └── api-contract.md
├── .gitignore
└── README.md
```

## バックエンド構成の考え方

### `ProductInventory.Api`

HTTP 入出力に責務を限定します。Controller、リクエスト/レスポンス DTO、認証・例外処理など、ASP.NET Core 固有の実装を配置します。

- `Controllers/`: REST API のエンドポイントを配置する。
- `Contracts/`: フロントエンドへ公開するリクエスト/レスポンス型を配置する。
- `Middleware/`: 例外ハンドリング、リクエストログなど横断的な HTTP 処理を配置する。
- `Extensions/`: DI 登録や OpenAPI 設定など `Program.cs` を薄く保つための拡張メソッドを配置する。

### `ProductInventory.Application`

ユースケース、入力検証、トランザクション制御の入口を配置します。商品登録時に `Product` と初期 `Stock` を同時に作成する処理や、在庫取引登録時に `Stock.quantity` と `StockTransaction` を一貫して更新する処理はここに置きます。

### `ProductInventory.Domain`

`User`, `Product`, `Stock`, `StockTransaction`、列挙値、ドメイン上の制約を配置します。ASP.NET Core や Entity Framework Core への依存を避け、共通仕様のデータモデルを表現する層にします。

### `ProductInventory.Infrastructure`

永続化、認証の具体実装、Repository など外部技術に依存するコードを配置します。初期データ投入、マイグレーション、テスト用データ作成もここにまとめます。

### `ProductInventory.Tests`

まずは Application 層のユースケーステストと、API の最小統合テストを置きます。比較用の CRUD アプリなので、網羅性よりも「要件を満たしているか」が確認しやすい配置を優先します。

## フロントエンド構成の考え方

### `src/app`

Next.js App Router のルーティングを配置します。ログイン画面は `(auth)`、ログイン後に利用する商品・在庫画面は `(protected)` に分け、認証ガードを `layout.tsx` に寄せます。

### `src/components`

表示に寄った再利用部品を配置します。業務別コンポーネントは `products/`, `stocks/`, `stock-transactions/` に分け、ボタンや入力欄など汎用 UI は `ui/` に置きます。

### `src/features`

画面横断の業務ロジックを配置します。検索条件、フォーム状態、API 呼び出しを組み合わせる hook や helper は、対象ドメイン別にまとめます。

### `src/lib`

API クライアント、認証状態の取得、表示フォーマットなど、特定画面に閉じない共通処理を配置します。

### `src/schemas` と `src/types`

フォームバリデーションや API レスポンスの型を配置します。バックエンドの `Contracts/` と対応関係が分かる命名にして、API 契約のズレを発見しやすくします。

## ルーティング案

| 画面 | Next.js パス | 主な API |
| --- | --- | --- |
| ログイン | `/login` | `POST /api/auth/login` |
| 商品一覧 | `/products` | `GET /api/products` |
| 商品詳細 | `/products/[productId]` | `GET /api/products/{id}` |
| 商品登録 | `/products/new` | `POST /api/products` |
| 商品編集 | `/products/[productId]/edit` | `PUT /api/products/{id}` |
| 在庫編集 | `/products/[productId]/stock/edit` | `PUT /api/stocks/{id}` |
| 在庫取引登録 | `/products/[productId]/stock-transactions/new` | `POST /api/stock-transactions` |
| 在庫取引一覧 | `/stock-transactions` | `GET /api/stock-transactions` |

## API と認証の置き場所

- 認証は初期実装では Cookie ベースを第一候補にする。
- バックエンドの認証設定は `ProductInventory.Infrastructure/Authentication/` と `ProductInventory.Api/Extensions/` に分ける。
- フロントエンドの認証状態確認は `src/lib/auth.ts`、ログインフォーム関連は `src/features/auth/` に置く。
- CORS、Cookie、CSRF の方針は `docs/api-contract.md` または実装メモに明記する。

## 初期実装の作成順

1. `backend/ProductInventory.sln` と4プロジェクトを作成する。
2. `Domain` に共通仕様のエンティティと enum を定義する。
3. `Infrastructure` に EF Core の `AppDbContext`、マイグレーション、初期データを作成する。
4. `Application` に Product 作成・一覧・詳細と StockTransaction 登録のユースケースを作成する。
5. `Api` に Auth、Products、Stocks、StockTransactions の Controller を作成する。
6. `frontend/` に Next.js App Router プロジェクトを作成する。
7. ログイン、商品一覧、商品詳細、商品登録、在庫取引登録の順に画面を実装する。
8. バックエンド統合テストとフロントエンドのフォーム/表示テストを追加する。
