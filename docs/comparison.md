# 実装済み構成の比較表

各構成の実装状況と主要な比較観点を記録するための一覧表。

| Stack | Status | DB | Migration | Validation | Auth | Test | Notes |
|---|---|---|---|---|---|---|---|
| ASP.NET Core Web API + React/Next.js | WIP | TBD | TBD | TBD | TBD | TBD | 商品詳細は [BFF で複数 API を集約](#aspnet-core--nextjs-の商品詳細取得)（共通仕様との差分）。 |
| T3 Stack | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |
| Spring Boot REST API + React/Next.js | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |
| ASP.NET Core MVC + Razor | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |
| Spring Boot + Thymeleaf | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |
| ASP.NET Core Minimal API | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |
| NestJS + Prisma | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |
| Rust Axum + SQLx | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |
| ASP.NET Core Clean Architecture | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |
| Spring Boot Hexagonal Architecture | Todo | TBD | TBD | TBD | TBD | TBD | 未実装。 |

## Status の定義

- `Todo`: 未着手。
- `WIP`: 実装中、または一部のみ完了。
- `Done`: 共通仕様に対する実装と確認が完了。

## 設計差分

### ASP.NET Core + Next.js の商品詳細取得

#### 採用した設計

共通仕様では、バックエンドの `GET /products/{id}` で商品、在庫、在庫取引履歴を一度に取得することを想定している。これに対し、この構成では **Next.js の BFF（Backend for Frontend）が画面用のレスポンスを組み立てる方式**を採用した。

商品詳細画面の BFF は、バックエンドの次の API を並行して呼び出し、結果を一つにまとめて UI へ返す。

- `GET /products/{id}`: 商品詳細
- `GET /stocks?product_id={id}&page=1&page_size=1`: 商品の在庫
- `GET /stock-transactions?product_id={id}&page=1&page_size=20`: 商品別の直近の在庫取引履歴

したがって、商品詳細画面は商品、在庫、直近の在庫取引履歴を一度の画面用リクエストで表示でき、画面要件を満たす。一方、バックエンドの `GET /products/{id}` 単体には在庫取引履歴が含まれないため、共通 API 仕様とは意図的に異なる。

#### 採用理由

- **画面固有の取得要件を BFF に閉じ込められる。** 「直近 20 件」のような表示都合を Product API の契約へ持ち込まず、ドメイン単位のバックエンド API を保てる。
- **履歴の肥大化から商品 API を守れる。** 在庫取引は増え続けるため、商品詳細レスポンスへ常に内包すると、応答サイズ、クエリ負荷、ページング仕様が商品 API に波及する。独立した一覧 API なら件数制限やページングを明示できる。
- **クライアントとバックエンド間の調整役を一か所にできる。** ブラウザが三つの API とそれぞれ通信するのではなく、認証情報の中継、並行取得、レスポンス整形、エラー変換を Next.js 側に集約できる。
- **既存 API を再利用しやすい。** 在庫一覧と在庫取引一覧は、商品詳細以外の画面や将来のクライアントからも同じ検索・ページング契約で利用できる。

この判断は「常に BFF の方が優れる」という意味ではない。呼び出しが三つになるため、バックエンド内で一度に取得する API と比べて通信回数が増え、部分的な失敗への扱いも必要になる。また、三つの読み取りの間に在庫更新が発生すると、商品詳細画面の在庫数と最新取引が厳密に同一時点のスナップショットにならない可能性がある。現在は独立した読み取り API を並行実行し、画面固有の集約と履歴ページングを優先するため、このトレードオフを許容する。

#### 見直す条件

次のいずれかが要件になった場合は、BFF での複数 API 集約をやめるのではなく、まずバックエンドに商品詳細画面専用の集約クエリ API を追加し、BFF からその API を呼ぶ構成を検討する。

- 商品、在庫、取引履歴に厳密な同一時点の整合性が必要
- API 間通信のレイテンシーやバックエンド負荷が無視できない
- BFF 以外の複数クライアントも、まったく同じ集約レスポンスを必要とする
- 部分失敗時のフォールバックが複雑になり、BFF の責務が過大になる

現時点では BFF をやめる必要はない。画面用 API とドメイン API の責務を分離できる利点があり、取得処理も並行化されているためである。ただし、上記の整合性または性能要件が生じた時点で、計測結果を基にバックエンド集約 API へ寄せる。
