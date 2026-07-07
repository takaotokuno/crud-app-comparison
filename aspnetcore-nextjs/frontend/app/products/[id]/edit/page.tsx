"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ProductForm } from "@/components/ProductForm";
import { requestJson, toOptionalValue } from "@/lib/api";
import { ProductDetail, ProductFormState } from "@/lib/types";

export default function EditProductPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [message, setMessage] = useState("商品情報を取得中です...");

  useEffect(() => {
    requestJson<ProductDetail>(`/api/products/${id}`)
      .then((data) => {
        setProduct(data);
        setMessage("");
      })
      .catch((error) => setMessage(error instanceof Error ? error.message : "商品情報の取得に失敗しました。"));
  }, [id]);

  async function updateProduct(form: ProductFormState) {
    const updated = await requestJson<ProductDetail>(`/api/products/${id}`, {
      method: "PUT",
      body: JSON.stringify({ id, sku: form.sku.trim(), name: form.name.trim(), description: toOptionalValue(form.description), category: toOptionalValue(form.category), price: Number(form.price), status: form.status }),
    });
    router.push(`/products/${updated.id}`);
  }

  return (
    <main className="mx-auto w-full max-w-3xl px-6 py-8 text-slate-900">
      <h1 className="mb-4 text-2xl font-semibold">商品編集</h1>
      {message && <p className="rounded bg-slate-50 px-3 py-2 text-sm text-slate-700">{message}</p>}
      {product && <ProductForm initialValue={product} submitLabel="更新する" onSubmit={updateProduct} />}
    </main>
  );
}
