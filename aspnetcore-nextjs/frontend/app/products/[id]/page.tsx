"use client";

import {
  Alert, Breadcrumbs, Button, Container, Group, Paper,
  SimpleGrid, Stack, Table, Text, Title,
} from "@mantine/core";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useHasRole } from "@/components/AuthProvider";
import { requestJson } from "@/lib/api";
import { StockPageResponse, statusLabels, transactionTypeLabels } from "@/lib/types";

export default function ProductDetailPage() {
  const canManageProducts = useHasRole(0);
  const canManageStock = useHasRole(0, 1);
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [data, setData] = useState<StockPageResponse | null>(null);
  const [message, setMessage] = useState("商品詳細を取得中です...");

  useEffect(() => {
    requestJson<StockPageResponse>(`/api/bff/products/${id}/stock-page`)
      .then((response) => {
        setData(response);
        setMessage("");
      })
      .catch((error) => {
        setMessage(error instanceof Error ? error.message : "商品詳細の取得に失敗しました。");
      });
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

  const product = data?.product;
  return (
    <Container component="main" size="md" py="xl">
      <Stack gap="lg">
        <Breadcrumbs><Link href="/products">商品一覧</Link><span>商品詳細</span></Breadcrumbs>
        <Group justify="space-between" align="flex-start">
          <Title order={1}>商品詳細</Title>
          <Group>
            <Button component={Link} href="/products" variant="default">一覧へ</Button>
            {product && canManageProducts && (
              <Button component={Link} href={`/products/${id}/edit`} variant="default">編集</Button>
            )}
            {product && canManageStock && (
              <Button component={Link} href={`/products/${id}/stock`}>在庫操作</Button>
            )}
            {canManageProducts && (
              <Button color="red" onClick={deleteProduct} disabled={!product}>削除</Button>
            )}
          </Group>
        </Group>
        {message && <Alert>{message}</Alert>}
        {product && (
          <>
            <Paper p="lg" withBorder>
              <Title order={2} size="h3" mb="md">商品情報</Title>
              <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="md">
                <Info label="SKU" value={product.sku} mono />
                <Info label="商品名" value={product.name} />
                <Info label="カテゴリ" value={product.category ?? "-"} />
                <Info label="価格" value={`¥${product.price.toLocaleString()}`} />
                <Info label="ステータス" value={statusLabels[product.status]} />
                <Info label="説明" value={product.description ?? "-"} />
                <Info label="作成日時" value={new Date(product.createdAt).toLocaleString()} />
                <Info label="更新日時" value={new Date(product.updatedAt).toLocaleString()} />
              </SimpleGrid>
            </Paper>
            <Paper p="lg" withBorder>
              <Title order={2} size="h3" mb="md">在庫情報</Title>
              <SimpleGrid cols={{ base: 1, sm: 3 }}>
                <Info label="現在在庫数" value={String(product.quantity)} />
                <Info label="安全在庫数" value={String(product.safetyStock)} />
                <Info label="在庫更新日時" value={new Date(product.updatedAt).toLocaleString()} />
              </SimpleGrid>
            </Paper>
            <Paper p="lg" withBorder>
              <Title order={2} size="h3" mb="md">直近の在庫取引</Title>
              <Table.ScrollContainer minWidth={600}>
                <Table>
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>日時</Table.Th><Table.Th>種別</Table.Th>
                      <Table.Th>増減数</Table.Th><Table.Th>取引後在庫</Table.Th>
                      <Table.Th>理由</Table.Th><Table.Th>操作者</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {data.transactions.items.map((transaction) => (
                      <Table.Tr key={transaction.id}>
                        <Table.Td>{new Date(transaction.createdAt).toLocaleString()}</Table.Td>
                        <Table.Td>{transactionTypeLabels[transaction.type]}</Table.Td>
                        <Table.Td>{transaction.quantityDelta}</Table.Td>
                        <Table.Td>{transaction.quantityAfter}</Table.Td>
                        <Table.Td>{transaction.reason ?? "-"}</Table.Td>
                        <Table.Td>{transaction.createdById ?? "-"}</Table.Td>
                      </Table.Tr>
                    ))}
                    {data.transactions.items.length === 0 && (
                      <Table.Tr><Table.Td colSpan={6} ta="center">在庫取引履歴がありません。</Table.Td></Table.Tr>
                    )}
                  </Table.Tbody>
                </Table>
              </Table.ScrollContainer>
            </Paper>
          </>
        )}
      </Stack>
    </Container>
  );
}

function Info({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return (
    <div>
      <Text size="sm" c="dimmed" fw={500}>{label}</Text>
      <Text ff={mono ? "monospace" : undefined}>{value}</Text>
    </div>
  );
}
