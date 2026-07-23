"use client";

import {
  Alert, Breadcrumbs, Button, Checkbox, Container, Group, Paper,
  Pagination, Select, Stack, Table, Text, TextInput, Title,
} from "@mantine/core";
import Link from "next/link";
import { useMemo, useState } from "react";
import { requestJson } from "@/lib/api";
import { ProductListResponse, ProductSummary, statusLabels } from "@/lib/types";

const PAGE_SIZE = 20;

export default function ProductsPage() {
  const [products, setProducts] = useState<ProductSummary[]>([]);
  const [query, setQuery] = useState("");
  const [status, setStatus] = useState<string | null>(null);
  const [category, setCategory] = useState("");
  const [lowStock, setLowStock] = useState(false);
  const [sortBy, setSortBy] = useState<string | null>("updated_at");
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [message, setMessage] = useState("検索条件を指定して商品一覧を取得してください。");
  const [isLoading, setIsLoading] = useState(false);

  const totalStock = useMemo(
    () => products.reduce((total, product) => total + product.quantity, 0),
    [products],
  );

  async function loadProducts(nextPage = 1) {
    setIsLoading(true);
    setMessage("商品一覧を取得中です...");
    try {
      const searchParams = new URLSearchParams({
        page: String(nextPage),
        page_size: String(PAGE_SIZE),
        sort_by: sortBy ?? "updated_at",
        sort_direction: "desc",
      });
      if (query.trim()) searchParams.set("q", query.trim());
      if (status) searchParams.set("status", status);
      if (category.trim()) searchParams.set("category", category.trim());
      if (lowStock) searchParams.set("low_stock", "true");
      const data = await requestJson<ProductListResponse>(
        `/api/products?${searchParams.toString()}`,
      );
      setProducts(data.items);
      setPage(data.page);
      setTotalCount(data.totalCount);
      setMessage(`${data.totalCount} 件中 ${data.items.length} 件を表示しています。`);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "商品一覧の取得に失敗しました。");
    } finally {
      setIsLoading(false);
    }
  }

  function clearFilters() {
    setQuery("");
    setStatus(null);
    setCategory("");
    setLowStock(false);
    setSortBy("updated_at");
  }

  return (
    <Container component="main" size="lg" py="xl">
      <Stack gap="lg">
        <Breadcrumbs><span>商品一覧</span></Breadcrumbs>
        <Group justify="space-between">
          <Title order={1}>商品一覧</Title>
          <Button component={Link} href="/products/new">新規登録</Button>
        </Group>
        <Paper p="md" withBorder>
          <Stack>
            <Group align="end" grow>
              <TextInput
                label="検索キーワード"
                placeholder="SKU・商品名・説明"
                value={query}
                onChange={(event) => setQuery(event.currentTarget.value)}
              />
              <Select
                label="ステータス"
                placeholder="すべて"
                value={status}
                onChange={setStatus}
                clearable
                data={[
                  { value: "0", label: "販売中" },
                  { value: "1", label: "停止中" },
                  { value: "2", label: "廃番" },
                ]}
              />
              <TextInput
                label="カテゴリ"
                value={category}
                onChange={(event) => setCategory(event.currentTarget.value)}
              />
              <Select
                label="並び順"
                value={sortBy}
                onChange={setSortBy}
                allowDeselect={false}
                data={[
                  { value: "updated_at", label: "更新日時" },
                  { value: "sku", label: "SKU" },
                  { value: "name", label: "商品名" },
                  { value: "price", label: "価格" },
                  { value: "quantity", label: "在庫数" },
                ]}
              />
            </Group>
            <Group justify="space-between">
              <Checkbox
                label="在庫不足のみ"
                checked={lowStock}
                onChange={(event) => setLowStock(event.currentTarget.checked)}
              />
              <Group>
                <Button variant="default" onClick={clearFilters}>条件をクリア</Button>
                <Button loading={isLoading} onClick={() => loadProducts(1)}>検索</Button>
              </Group>
            </Group>
            <Alert>{message}</Alert>
          </Stack>
        </Paper>
        <Group justify="flex-end">
          <Text size="sm" c="dimmed">表示数: {products.length}／在庫合計: {totalStock}</Text>
        </Group>
        <Table.ScrollContainer minWidth={900}>
          <Table striped highlightOnHover withTableBorder>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>SKU</Table.Th><Table.Th>商品名</Table.Th><Table.Th>カテゴリ</Table.Th>
                <Table.Th>価格</Table.Th><Table.Th>在庫</Table.Th><Table.Th>安全在庫</Table.Th>
                <Table.Th>状態</Table.Th><Table.Th>在庫更新日時</Table.Th><Table.Th>操作</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {products.map((product) => (
                <ProductRow key={product.id} product={product} />
              ))}
              {products.length === 0 && (
                <Table.Tr>
                  <Table.Td colSpan={9} ta="center" py="xl">
                    商品データがありません。条件を変更するか、新規登録してください。
                  </Table.Td>
                </Table.Tr>
              )}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
        {totalCount > PAGE_SIZE && (
          <Pagination
            value={page}
            total={Math.ceil(totalCount / PAGE_SIZE)}
            onChange={(nextPage) => loadProducts(nextPage)}
            mx="auto"
          />
        )}
      </Stack>
    </Container>
  );
}

function ProductRow({ product }: { product: ProductSummary }) {
  const isLowStock = product.quantity <= product.safetyStock;
  return (
    <Table.Tr bg={isLowStock ? "var(--mantine-color-yellow-0)" : undefined}>
      <Table.Td ff="monospace">{product.sku}</Table.Td>
      <Table.Td>{product.name}</Table.Td><Table.Td>{product.category ?? "-"}</Table.Td>
      <Table.Td>¥{product.price.toLocaleString()}</Table.Td>
      <Table.Td c={isLowStock ? "red" : undefined} fw={700}>{product.quantity}</Table.Td>
      <Table.Td>{product.safetyStock}</Table.Td><Table.Td>{statusLabels[product.status]}</Table.Td>
      <Table.Td>{new Date(product.updatedAt).toLocaleString()}</Table.Td>
      <Table.Td>
        <Group gap="xs" wrap="nowrap">
          <Button component={Link} href={`/products/${product.id}`} size="xs" variant="default">詳細</Button>
          <Button component={Link} href={`/products/${product.id}/edit`} size="xs" variant="default">編集</Button>
          <Button component={Link} href={`/products/${product.id}/stock`} size="xs" variant="default">在庫操作</Button>
        </Group>
      </Table.Td>
    </Table.Tr>
  );
}
