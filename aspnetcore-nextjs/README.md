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
