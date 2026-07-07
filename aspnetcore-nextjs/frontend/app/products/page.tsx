"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { requestJson } from "@/lib/api";
import { ProductListResponse, ProductStatus, ProductSummary, statusLabels } from "@/lib/types";

export default function ProductsPage() {
  const [products, setProducts] = useState<ProductSummary[]>([]);
  const [query, setQuery] = useState("");
  const [status, setStatus] = useState("");
  const [lowStock, setLowStock] = useState(false);
  const [message, setMessage] = useState("検索条件を指定して商品一覧を取得してください。");
  const [isLoading, setIsLoading] = useState(false);

  const totalStock = useMemo(() => products.reduce((total, product) => total + product.quantity, 0), [products]);

  async function loadProducts() {
    setIsLoading(true);
    setMessage("商品一覧を取得中です...");
    try {
      const searchParams = new URLSearchParams({ page: "1", page_size: "20", sort_by: "updated_at", sort_direction: "desc" });
      if (query.trim()) searchParams.set("q", query.trim());
      if (status) searchParams.set("status", status);
      if (lowStock) searchParams.set("low_stock", "true");
      const data = await requestJson<ProductListResponse>(`/api/products?${searchParams.toString()}`);
      setProducts(data.items);
      setMessage(`${data.totalCount} 件中 ${data.items.length} 件の商品を取得しました。`);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "商品一覧の取得に失敗しました。");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="mx-auto w-full max-w-6xl px-6 py-8 text-slate-900">
      <section className="rounded border border-slate-200 bg-white p-4">
        <div className="grid gap-3 md:grid-cols-[1fr_180px_auto_auto] md:items-end">
          <label className="flex flex-col gap-1 text-sm font-medium">検索キーワード<input className="rounded border border-slate-300 px-3 py-2 font-normal" value={query} onChange={(event) => setQuery(event.target.value)} placeholder="SKU / 商品名 / 説明" /></label>
          <label className="flex flex-col gap-1 text-sm font-medium">ステータス<select className="rounded border border-slate-300 px-3 py-2 font-normal" value={status} onChange={(event) => setStatus(event.target.value)}><option value="">すべて</option><option value="0">販売中</option><option value="1">停止中</option><option value="2">廃番</option></select></label>
          <label className="flex items-center gap-2 pb-2 text-sm"><input checked={lowStock} onChange={(event) => setLowStock(event.target.checked)} type="checkbox" />安全在庫以下のみ</label>
          <button className="rounded bg-slate-900 px-4 py-2 text-white disabled:opacity-50" onClick={loadProducts} disabled={isLoading}>一覧取得</button>
        </div>
        <p className="mt-3 text-sm text-slate-600">{message}</p>
      </section>

      <section className="mt-8 space-y-4">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold">商品一覧</h1>
          <p className="text-sm text-slate-500">表示数: {products.length} / 在庫合計: {totalStock}</p>
        </div>
        <div className="overflow-x-auto rounded border border-slate-200 bg-white">
          <table className="min-w-full border-collapse text-left text-sm">
            <thead className="bg-slate-50"><tr><th className="border-b border-slate-200 px-3 py-2">SKU</th><th className="border-b border-slate-200 px-3 py-2">商品名</th><th className="border-b border-slate-200 px-3 py-2">カテゴリ</th><th className="border-b border-slate-200 px-3 py-2">価格</th><th className="border-b border-slate-200 px-3 py-2">現在在庫</th><th className="border-b border-slate-200 px-3 py-2">安全在庫</th><th className="border-b border-slate-200 px-3 py-2">状態</th><th className="border-b border-slate-200 px-3 py-2">在庫更新日時</th><th className="border-b border-slate-200 px-3 py-2">操作</th></tr></thead>
            <tbody>
              {products.map((product) => {
                const isLowStock = product.quantity <= product.safetyStock;
                return <tr className={isLowStock ? "bg-amber-50" : undefined} key={product.id}><td className="border-b border-slate-100 px-3 py-2 font-mono">{product.sku}</td><td className="border-b border-slate-100 px-3 py-2">{product.name}</td><td className="border-b border-slate-100 px-3 py-2">{product.category ?? "-"}</td><td className="border-b border-slate-100 px-3 py-2">¥{product.price.toLocaleString()}</td><td className="border-b border-slate-100 px-3 py-2 font-semibold">{product.quantity}</td><td className="border-b border-slate-100 px-3 py-2">{product.safetyStock}</td><td className="border-b border-slate-100 px-3 py-2">{statusLabels[product.status]}</td><td className="border-b border-slate-100 px-3 py-2">{new Date(product.updatedAt).toLocaleString()}</td><td className="border-b border-slate-100 px-3 py-2"><div className="flex gap-2"><Link className="rounded border border-slate-300 px-2 py-1" href={`/products/${product.id}`}>詳細</Link><Link className="rounded border border-slate-300 px-2 py-1" href={`/products/${product.id}/edit`}>編集</Link><button className="rounded border border-slate-300 px-2 py-1 text-slate-400" disabled>在庫取引</button></div></td></tr>;
              })}
              {products.length === 0 && <tr><td className="px-3 py-8 text-center text-slate-500" colSpan={9}>商品データがありません。一覧取得または条件変更をしてください。</td></tr>}
            </tbody>
          </table>
        </div>
      </section>
    </main>
  );
}
