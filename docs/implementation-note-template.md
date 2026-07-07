# 実装メモのテンプレート

各実装フォルダには、以下の内容を含む実装メモを置く。

```markdown
# 実装メモ

## 構成

- 言語:
- フレームワーク:
- DB:
- ORM/Query Builder:
- 認証方式:
- テスト:

## 起動手順

## テスト手順

## データモデル実装方針

- Product:
- Stock:
- StockTransaction:
- 削除方針:
  - Product の削除操作は論理削除（`IsDeleted = true` または `Status = Discontinued`）として扱うか。
  - Product / Stock / StockTransaction の物理削除可否と外部キー `ON DELETE` 設定。
  - StockTransaction が存在する Product / Stock は履歴保持のため物理削除しないこと。

## 共通仕様との差分

## 学び・所感
```
