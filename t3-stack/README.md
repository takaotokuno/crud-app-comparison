# T3 Stack

T3 Stack（Next.js、TypeScript、tRPC、Prisma、NextAuth.js）で CRUD アプリを実装するための作業フォルダです。

## 想定構成

```text
t3-stack/
├── .devcontainer/
│   └── devcontainer.json
├── app/          # create-t3-app で作成するアプリ本体
├── docs/
│   ├── comparison-checkpoints.md
│   └── implementation-note-template.md
├── .gitignore
└── README.md
```

## Dev Container

このフォルダには、Node.js 20 を利用できる Dev Container 設定を含めています。

1. VS Code でこのリポジトリを開きます。
2. `t3-stack` フォルダをワークスペースとして開くか、Dev Containers 拡張機能からこのフォルダの設定を選択します。
3. **Reopen in Container** を実行します。

公開ポート:

- `3000`: Next.js
- `5555`: Prisma Studio

## 初期化例

T3 Stack のプロジェクトは、必要に応じて次のように作成できます。

```bash
npm create t3-app@latest app
```

作成時の選択例:

- TypeScript: Yes
- tRPC: Yes
- Prisma: Yes
- NextAuth.js: Yes
- Tailwind CSS: 任意
- App Router: 任意

## 実行例

依存関係のインストール:

```bash
cd app
npm install
```

開発サーバー:

```bash
npm run dev
```

Prisma Studio:

```bash
npx prisma studio
```

## メモ

- API の仕様、画面、CRUD 要件はリポジトリの `docs/` 配下の資料に合わせて実装します。
- 認証は NextAuth.js を基本とし、採用したプロバイダーやセッション方式は実装メモに記録します。
- ローカル専用の環境変数は `.env` または `.env.*` に配置し、必要なキーの例は `.env.example` に記載します。
