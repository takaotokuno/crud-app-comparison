"use client";

import { Alert, Button, Group, NumberInput, Paper, Select, Stack, Textarea, TextInput } from "@mantine/core";
import Link from "next/link";
import { FormEvent, useState } from "react";
import { ProductDetail, ProductFormState, ProductStatus, initialFormState } from "@/lib/types";

type Props = {
  initialValue?: ProductDetail;
  submitLabel: string;
  cancelHref: string;
  onSubmit: (form: ProductFormState) => Promise<void>;
};

export function ProductForm({ initialValue, submitLabel, cancelHref, onSubmit }: Props) {
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

  function update<K extends keyof ProductFormState>(key: K, value: ProductFormState[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }

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
    <Paper component="form" onSubmit={handleSubmit} p="lg" withBorder>
      <Stack>
        {error && <Alert color="red">{error}</Alert>}
        <TextInput
          label="SKU" value={form.sku} required
          onChange={(event) => update("sku", event.currentTarget.value)}
        />
        <TextInput
          label="商品名" value={form.name} required
          onChange={(event) => update("name", event.currentTarget.value)}
        />
        <Textarea
          label="説明" value={form.description} rows={4}
          onChange={(event) => update("description", event.currentTarget.value)}
        />
        <TextInput
          label="カテゴリ" value={form.category}
          onChange={(event) => update("category", event.currentTarget.value)}
        />
        <NumberInput
          label="価格（円）" value={form.price} min={0} required
          onChange={(value) => update("price", String(value))}
        />
        {!initialValue && (
          <Group grow align="start">
            <NumberInput
              label="初期在庫数"
              value={form.initialQuantity}
              onChange={(value) => update("initialQuantity", String(value))}
              min={0}
              required
            />
            <NumberInput
              label="安全在庫数"
              value={form.safetyStock}
              onChange={(value) => update("safetyStock", String(value))}
              min={0}
              required
            />
          </Group>
        )}
        <Select
          label="商品ステータス"
          value={String(form.status)}
          onChange={(value) => update("status", Number(value) as ProductStatus)}
          data={[{ value: "0", label: "販売中" }, { value: "1", label: "停止中" }, { value: "2", label: "廃番" }]}
          allowDeselect={false}
          required
        />
        <Group justify="flex-end">
          <Button component={Link} href={cancelHref} variant="default">キャンセル</Button>
          <Button loading={isSaving} type="submit">{submitLabel}</Button>
        </Group>
      </Stack>
    </Paper>
  );
}
