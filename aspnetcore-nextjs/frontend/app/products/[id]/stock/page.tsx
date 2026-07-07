"use client";

import Link from "next/link";
import { FormEvent, useEffect, useMemo, useState } from "react";
import { useParams } from "next/navigation";
import { requestJson, toOptionalValue } from "@/lib/api";
import { ProductDetail, StockDetail, StockListResponse, StockTransactionListResponse, StockTransactionType, transactionTypeLabels } from "@/lib/types";

export default function ProductStockPage() {
  const { id } = useParams<{ id: string }>();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [stock, setStock] = useState<StockDetail | null>(null);
  const [transactions, setTransactions] = useState<StockTransactionListResponse | null>(null);
  const [quantity, setQuantity] = useState("0");
  const [safetyStock, setSafetyStock] = useState("0");
  const [stockReason, setStockReason] = useState("");
  const [transactionType, setTransactionType] = useState<StockTransactionType>(0);
  const [transactionQuantity, setTransactionQuantity] = useState("1");
  const [transactionReason, setTransactionReason] = useState("");
  const [message, setMessage] = useState("在庫情報を取得中です...");
  const [isSavingStock, setIsSavingStock] = useState(false);
  const [isSavingTransaction, setIsSavingTransaction] = useState(false);

  const quantityDelta = useMemo(() => {
    const amount = Number(transactionQuantity);
    if (!Number.isFinite(amount)) return 0;
    return transactionType === 1 ? -Math.abs(amount) : Math.abs(amount);
  }, [transactionQuantity, transactionType]);

  async function loadStockPage() {
    setMessage("在庫情報を取得中です...");
    try {
      const [productData, stockData, transactionData] = await Promise.all([
        requestJson<ProductDetail>(`/api/products/${id}`),
        requestJson<StockListResponse>(`/api/stocks?product_id=${id}&page=1&page_size=1`),
        requestJson<StockTransactionListResponse>(`/api/stock-transactions?product_id=${id}&page=1&page_size=20`),
      ]);
      const firstStock = stockData.items[0] ?? null;
      setProduct(productData);
      setStock(firstStock);
      setTransactions(transactionData);
      if (firstStock) {
        setQuantity(String(firstStock.quantity));
        setSafetyStock(String(firstStock.safetyStock));
      }
      setMessage(firstStock ? "" : "この商品の在庫情報が見つかりません。");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "在庫情報の取得に失敗しました。");
    }
  }

  useEffect(() => {
    void loadStockPage();
  }, [id]);

  async function updateStock(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!stock) return;
    setIsSavingStock(true);
    setMessage("");
    try {
      const updated = await requestJson<StockDetail>(`/api/stocks/${stock.id}`, {
        method: "PUT",
        body: JSON.stringify({ quantity: Number(quantity), safetyStock: Number(safetyStock), reason: toOptionalValue(stockReason) }),
      });
      setStock(updated);
      setQuantity(String(updated.quantity));
      setSafetyStock(String(updated.safetyStock));
      setStockReason("");
      setMessage("現在在庫数と安全在庫数を更新しました。");
      await loadStockPage();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "在庫更新に失敗しました。");
    } finally {
      setIsSavingStock(false);
    }
  }

  async function createTransaction(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSavingTransaction(true);
    setMessage("");
    try {
      await requestJson("/api/stock-transactions", {
        method: "POST",
        body: JSON.stringify({ productId: id, type: transactionType, quantityDelta, reason: toOptionalValue(transactionReason) }),
      });
      setTransactionQuantity("1");
      setTransactionReason("");
      setMessage("在庫取引を登録しました。");
      await loadStockPage();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "在庫取引登録に失敗しました。");
    } finally {
      setIsSavingTransaction(false);
    }
  }

  return (
    <main className="mx-auto w-full max-w-5xl px-6 py-8 text-slate-900">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-semibold">在庫管理</h1>{product && <p className="text-sm text-slate-500">{product.sku} / {product.name}</p>}</div><div className="flex gap-2"><Link className="rounded border border-slate-300 px-3 py-2" href="/products">一覧へ</Link><Link className="rounded border border-slate-300 px-3 py-2" href={`/products/${id}`}>商品詳細へ</Link></div></div>
      {message && <p className="mb-4 rounded bg-slate-50 px-3 py-2 text-sm text-slate-700">{message}</p>}
      <div className="grid gap-6 lg:grid-cols-2">
        <form className="space-y-4 rounded border border-slate-200 bg-white p-5" onSubmit={updateStock}>
          <h2 className="text-lg font-semibold">現在在庫・安全在庫の編集</h2>
          <div className="grid gap-4 sm:grid-cols-2"><label className="block text-sm font-medium">現在在庫数<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={quantity} onChange={(event) => setQuantity(event.target.value)} type="number" min="0" required disabled={!stock} /></label><label className="block text-sm font-medium">安全在庫数<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={safetyStock} onChange={(event) => setSafetyStock(event.target.value)} type="number" min="0" required disabled={!stock} /></label></div>
          <label className="block text-sm font-medium">更新理由<textarea className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={stockReason} onChange={(event) => setStockReason(event.target.value)} rows={3} placeholder="棚卸調整など" /></label>
          <button className="rounded bg-slate-900 px-4 py-2 text-white disabled:opacity-50" type="submit" disabled={!stock || isSavingStock}>在庫数を更新</button>
        </form>
        <form className="space-y-4 rounded border border-slate-200 bg-white p-5" onSubmit={createTransaction}>
          <h2 className="text-lg font-semibold">入庫・出庫・調整の登録</h2>
          <label className="block text-sm font-medium">取引種別<select className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={transactionType} onChange={(event) => setTransactionType(Number(event.target.value) as StockTransactionType)}><option value={0}>入庫</option><option value={1}>出庫</option><option value={2}>調整</option></select></label>
          <label className="block text-sm font-medium">数量<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={transactionQuantity} onChange={(event) => setTransactionQuantity(event.target.value)} type="number" min="1" required /></label>
          <p className="text-sm text-slate-500">登録される増減数: {quantityDelta}</p>
          <label className="block text-sm font-medium">理由<textarea className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={transactionReason} onChange={(event) => setTransactionReason(event.target.value)} rows={3} /></label>
          <button className="rounded bg-slate-900 px-4 py-2 text-white disabled:opacity-50" type="submit" disabled={!stock || isSavingTransaction}>在庫取引を登録</button>
        </form>
      </div>
      <section className="mt-6 rounded border border-slate-200 bg-white p-5"><h2 className="mb-3 text-lg font-semibold">直近の在庫取引履歴</h2><div className="overflow-x-auto"><table className="min-w-full text-left text-sm"><thead className="bg-slate-50"><tr><th className="border-b px-3 py-2">日時</th><th className="border-b px-3 py-2">種別</th><th className="border-b px-3 py-2">増減</th><th className="border-b px-3 py-2">取引後在庫</th><th className="border-b px-3 py-2">理由</th></tr></thead><tbody>{transactions?.items.map((transaction) => <tr key={transaction.id}><td className="border-b border-slate-100 px-3 py-2">{new Date(transaction.createdAt).toLocaleString()}</td><td className="border-b border-slate-100 px-3 py-2">{transactionTypeLabels[transaction.type]}</td><td className="border-b border-slate-100 px-3 py-2 font-semibold">{transaction.quantityDelta}</td><td className="border-b border-slate-100 px-3 py-2">{transaction.quantityAfter}</td><td className="border-b border-slate-100 px-3 py-2">{transaction.reason ?? "-"}</td></tr>)}{(!transactions || transactions.items.length === 0) && <tr><td className="px-3 py-8 text-center text-slate-500" colSpan={5}>在庫取引履歴がありません。</td></tr>}</tbody></table></div></section>
    </main>
  );
}
