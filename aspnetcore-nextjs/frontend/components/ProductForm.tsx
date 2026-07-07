"use client";

import { FormEvent, useState } from "react";
import { ProductDetail, ProductFormState, ProductStatus, initialFormState } from "@/lib/types";

type Props = {
  initialValue?: ProductDetail;
  submitLabel: string;
  onSubmit: (form: ProductFormState) => Promise<void>;
};

export function ProductForm({ initialValue, submitLabel, onSubmit }: Props) {
  const [form, setForm] = useState<ProductFormState>(
    initialValue
      ? {
          sku: initialValue.sku,
          name: initialValue.name,
          description: initialValue.description ?? "",
          category: initialValue.category ?? "",
          price: String(initialValue.price),
          status: initialValue.status,
          initialQuantity: String(initialValue.quantity),
          safetyStock: String(initialValue.safetyStock),
        }
      : initialFormState,
  );
  const [error, setError] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setIsSaving(true);
    try {
      await onSubmit(form);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "保存に失敗しました。");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <form className="space-y-4 rounded border border-slate-200 bg-white p-5" onSubmit={handleSubmit}>
      {error && <p className="rounded bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}
      <label className="block text-sm font-medium">SKU<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={form.sku} onChange={(event) => setForm({ ...form, sku: event.target.value })} required /></label>
      <label className="block text-sm font-medium">商品名<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required /></label>
      <label className="block text-sm font-medium">説明<textarea className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} rows={4} /></label>
      <label className="block text-sm font-medium">カテゴリ<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={form.category} onChange={(event) => setForm({ ...form, category: event.target.value })} /></label>
      <div className="grid gap-4 sm:grid-cols-3">
        <label className="block text-sm font-medium">価格<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={form.price} onChange={(event) => setForm({ ...form, price: event.target.value })} type="number" min="0" required /></label>
        <label className="block text-sm font-medium">初期/現在在庫<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={form.initialQuantity} onChange={(event) => setForm({ ...form, initialQuantity: event.target.value })} type="number" min="0" required disabled={Boolean(initialValue)} /></label>
        <label className="block text-sm font-medium">安全在庫<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={form.safetyStock} onChange={(event) => setForm({ ...form, safetyStock: event.target.value })} type="number" min="0" required disabled={Boolean(initialValue)} /></label>
      </div>
      <label className="block text-sm font-medium">商品ステータス<select className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={form.status} onChange={(event) => setForm({ ...form, status: Number(event.target.value) as ProductStatus })}><option value={0}>販売中</option><option value={1}>停止中</option><option value={2}>廃番</option></select></label>
      <button className="rounded bg-slate-900 px-4 py-2 text-white disabled:opacity-50" type="submit" disabled={isSaving}>{submitLabel}</button>
    </form>
  );
}
