"use client";

import { Alert, Breadcrumbs, Container, Stack, Title } from "@mantine/core";
import Link from "next/link";
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
      .catch((error) =>
        setMessage(error instanceof Error ? error.message : "商品情報の取得に失敗しました。"),
      );
  }, [id]);

  async function updateProduct(form: ProductFormState) {
    const updated = await requestJson<ProductDetail>(`/api/products/${id}`, {
      method: "PUT",
      body: JSON.stringify({
        id,
        sku: form.sku.trim(),
        name: form.name.trim(),
        description: toOptionalValue(form.description),
        category: toOptionalValue(form.category),
        price: Number(form.price),
        status: form.status,
      }),
    });
    router.push(`/products/${updated.id}`);
  }

  return (
    <Container component="main" size="sm" py="xl">
      <Stack>
        <Breadcrumbs>
          <Link href="/products">商品一覧</Link>
          <Link href={`/products/${id}`}>商品詳細</Link>
          <span>商品編集</span>
        </Breadcrumbs>
        <Title order={1}>商品編集</Title>
        {message && <Alert>{message}</Alert>}
        {product && (
          <ProductForm
            initialValue={product}
            submitLabel="更新する"
            cancelHref={`/products/${id}`}
            onSubmit={updateProduct}
          />
        )}
      </Stack>
    </Container>
  );
}
