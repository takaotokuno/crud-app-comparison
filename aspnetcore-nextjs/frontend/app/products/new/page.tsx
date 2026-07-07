"use client";

import { useRouter } from "next/navigation";
import { ProductForm } from "@/components/ProductForm";
import { requestJson, toOptionalValue } from "@/lib/api";
import { ProductDetail, ProductFormState } from "@/lib/types";

export default function NewProductPage() {
  const router = useRouter();

  async function createProduct(form: ProductFormState) {
    const product = await requestJson<ProductDetail>("/api/products", {
      method: "POST",
      body: JSON.stringify({ sku: form.sku.trim(), name: form.name.trim(), description: toOptionalValue(form.description), category: toOptionalValue(form.category), price: Number(form.price), status: form.status, initialQuantity: Number(form.initialQuantity), safetyStock: Number(form.safetyStock) }),
    });
    router.push(`/products/${product.id}`);
  }

  return <main className="mx-auto w-full max-w-3xl px-6 py-8 text-slate-900"><h1 className="mb-4 text-2xl font-semibold">商品登録</h1><ProductForm submitLabel="登録する" onSubmit={createProduct} /></main>;
}
