"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { requestJson } from "@/lib/api";
import { ProductDetail, statusLabels } from "@/lib/types";

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [message, setMessage] = useState("商品詳細を取得中です...");

  useEffect(() => {
    requestJson<ProductDetail>(`/api/products/${id}`).then((data) => { setProduct(data); setMessage(""); }).catch((error) => setMessage(error instanceof Error ? error.message : "商品詳細の取得に失敗しました。"));
  }, [id]);

  async function deleteProduct() {
    if (!confirm("この商品を削除しますか？")) return;
    try {
      await requestJson<void>(`/api/products/${id}`, { method: "DELETE" });
      router.push("/products");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "商品の削除に失敗しました。");
    }
  }

  return (
    <main className="mx-auto w-full max-w-4xl px-6 py-8 text-slate-900">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3"><h1 className="text-2xl font-semibold">商品詳細</h1><div className="flex gap-2"><Link className="rounded border border-slate-300 px-3 py-2" href="/products">一覧へ</Link>{product && <Link className="rounded border border-slate-300 px-3 py-2" href={`/products/${product.id}/edit`}>編集</Link>}<button className="rounded bg-red-600 px-3 py-2 text-white" onClick={deleteProduct} disabled={!product}>削除</button>{product && <Link className="rounded border border-slate-300 px-3 py-2" href={`/products/${product.id}/stock`}>在庫取引登録</Link>}</div></div>
      {message && <p className="rounded bg-slate-50 px-3 py-2 text-sm text-slate-700">{message}</p>}
      {product && <section className="grid gap-4 rounded border border-slate-200 bg-white p-5 sm:grid-cols-2"><Info label="ID" value={product.id} mono /><Info label="SKU" value={product.sku} mono /><Info label="商品名" value={product.name} /><Info label="カテゴリ" value={product.category ?? "-"} /><Info label="価格" value={`¥${product.price.toLocaleString()}`} /><Info label="ステータス" value={statusLabels[product.status]} /><Info label="現在在庫数" value={String(product.quantity)} /><Info label="安全在庫数" value={String(product.safetyStock)} /><Info label="作成日時" value={new Date(product.createdAt).toLocaleString()} /><Info label="更新日時" value={new Date(product.updatedAt).toLocaleString()} /><div className="sm:col-span-2"><Info label="説明" value={product.description ?? "-"} /></div><div className="sm:col-span-2 rounded bg-slate-50 p-4 text-sm text-slate-600">在庫取引履歴と現在在庫・安全在庫の編集は「在庫取引登録」から確認できます。</div></section>}
    </main>
  );
}

function Info({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return <div><dt className="text-sm font-medium text-slate-500">{label}</dt><dd className={mono ? "break-all font-mono" : ""}>{value}</dd></div>;
}
