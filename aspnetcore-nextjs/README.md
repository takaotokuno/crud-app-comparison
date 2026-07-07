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

バックエンド:

```bash
cd backend
dotnet run
```

フロントエンド:

```bash
cd frontend
npm run dev
```

## メモ

- API の仕様、画面、CRUD 要件はリポジトリの `docs/` 配下の資料に合わせて実装します。
- ローカル専用の環境変数は `.env` または `.env.*` に配置し、必要なキーの例は `.env.example` に記載します。


## 実装メモ

### データモデル実装方針

- Product は商品マスタとして扱い、Stock と StockTransaction の履歴参照元になるため物理削除しません。商品削除 API は `Products.Status = Discontinued` へ更新する論理削除として実装します。
- Stock は Product と 1:1 で保持し、Product の削除操作では削除しません。在庫数と安全在庫数は履歴整合性のため残します。
- StockTransaction は Product / Stock に紐づく在庫増減履歴として保持し、Product の削除操作では削除しません。StockTransaction が存在する Product / Stock に対して物理削除は行いません。

### 外部キーと削除制約

- 現在の EF Core 設定では Product → Stock は `DeleteBehavior.Cascade`、StockTransaction → Product は `DeleteBehavior.Restrict`、StockTransaction → Stock は `DeleteBehavior.Cascade` です。
- SQL Server で StockTransaction を作成した状態の Product を物理削除すると、StockTransaction → Product の Restrict により Product の削除は許可されません。Product → Stock → StockTransaction の Cascade 経路で履歴が消える設計にはしません。
- アプリケーション実装では Product 物理削除を実行せず、削除操作を `Status = Discontinued` への更新に統一することで、Stock / StockTransaction を履歴として保持します。
