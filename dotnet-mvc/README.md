# ASP.NET Core MVC（Feature Folder）

ASP.NET Core MVC で商品在庫管理 CRUD を実装するための基本構成です。.NET 8 を対象にしています。

## ディレクトリ構成

従来の `Controllers`、`Services`、`Repositories` のような技術レイヤー単位ではなく、変更理由が近いコードを機能単位でまとめる **Feature Folder** を採用します。

```text
dotnet-mvc/
├── Features/
│   └── Home/
│       ├── HomeController.cs
│       ├── HomeFeatureExtensions.cs  # 機能内の DI 登録
│       ├── Models/                   # ViewModel / 入出力モデル
│       ├── Services/                 # ユースケース、業務ロジック
│       ├── Infra/                    # DB・外部サービスなどの実装
│       └── Views/                    # HomeController の Razor View
├── Views/
│   └── Shared/                       # 機能横断の共通 View
├── _ViewImports.cshtml               # 全機能の View に適用
├── _ViewStart.cshtml                 # 全機能の View に適用
├── wwwroot/                          # 静的ファイル
├── Program.cs
└── DotnetMvc.csproj
```

今後は `Features/Products`、`Features/Stocks`、`Features/StockTransactions`、`Features/Auth` のように追加します。各機能が Controller、Service、Infra、Models、Views を所有するため、Controller が一つのディレクトリへ増え続けることを避けられます。

ASP.NET Core MVC は Controller の配置場所を限定しません。View は `Program.cs` で `/Features/{Controller}/Views/{View}.cshtml` を探索先の先頭に追加しています。機能名と Controller 名を揃えることを規約とし、共通 View は MVC 標準の `/Views/Shared` に置きます。

機能同士で再利用する必要が明確になった型だけを、将来 `Shared` や `Common` へ移します。最初から共通化せず、機能間の直接参照を増やさない方針です。

## 実行

```bash
dotnet restore
dotnet run
```

起動後、`http://localhost:5000` を開きます。
