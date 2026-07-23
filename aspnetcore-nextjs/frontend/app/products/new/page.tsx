"use client";

import { Breadcrumbs, Container, Stack, Title } from "@mantine/core";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ProductForm } from "@/components/ProductForm";
import { requestJson, toOptionalValue } from "@/lib/api";
import { ProductDetail, ProductFormState } from "@/lib/types";

export default function NewProductPage() {
  const router = useRouter();

  async function createProduct(form: ProductFormState) {
    const product = await requestJson<ProductDetail>("/api/products", {
      method: "POST",
      body: JSON.stringify({
        sku: form.sku.trim(),
        name: form.name.trim(),
        description: toOptionalValue(form.description),
        category: toOptionalValue(form.category),
        price: Number(form.price),
        status: form.status,
        initialQuantity: Number(form.initialQuantity),
        safetyStock: Number(form.safetyStock),
      }),
    });
    router.push(`/products/${product.id}`);
  }

  return (
    <Container component="main" size="sm" py="xl">
      <Stack>
        <Breadcrumbs><Link href="/products">商品一覧</Link><span>商品登録</span></Breadcrumbs>
        <Title order={1}>商品登録</Title>
        <ProductForm
          submitLabel="登録する"
          cancelHref="/products"
          onSubmit={createProduct}
        />
      </Stack>
    </Container>
  );
}
