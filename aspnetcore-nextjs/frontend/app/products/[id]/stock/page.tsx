"use client";

import {
  Alert, Breadcrumbs, Button, Container, Group, NumberInput, Paper,
  Select, SimpleGrid, Stack, Table, Text, Textarea, Title,
} from "@mantine/core";
import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useParams } from "next/navigation";
import { requestJson, toOptionalValue } from "@/lib/api";
import {
  ProductDetail, StockDetail, StockPageResponse, StockTransactionListResponse,
  StockTransactionType, transactionTypeLabels,
} from "@/lib/types";

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
      const data = await requestJson<StockPageResponse>(`/api/bff/products/${id}/stock-page`);
      setProduct(data.product);
      setStock(data.stock);
      setTransactions(data.transactions);
      if (data.stock) {
        setQuantity(String(data.stock.quantity));
        setSafetyStock(String(data.stock.safetyStock));
      }
      setMessage(data.stock ? "" : "この商品の在庫情報が見つかりません。");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "在庫情報の取得に失敗しました。");
    }
  }

  useEffect(() => {
    void loadStockPage();
  }, [id]);

  async function updateStock() {
    if (!stock) return;
    setIsSavingStock(true);
    setMessage("");
    try {
      const updated = await requestJson<StockDetail>(`/api/stocks/${stock.id}`, {
        method: "PUT",
        body: JSON.stringify({
          quantity: Number(quantity),
          safetyStock: Number(safetyStock),
          reason: toOptionalValue(stockReason),
        }),
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

  async function createTransaction() {
    setIsSavingTransaction(true);
    setMessage("");
    try {
      await requestJson("/api/stock-transactions", {
        method: "POST",
        body: JSON.stringify({
          productId: id,
          type: transactionType,
          quantityDelta,
          reason: toOptionalValue(transactionReason),
        }),
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
    <Container component="main" size="lg" py="xl">
      <Stack gap="lg">
        <Breadcrumbs>
          <Link href="/products">商品一覧</Link>
          <Link href={`/products/${id}`}>商品詳細</Link>
          <span>在庫操作</span>
        </Breadcrumbs>
        <Group justify="space-between">
          <div>
            <Title order={1}>在庫操作</Title>
            {product && <Text c="dimmed">{product.sku}／{product.name}</Text>}
          </div>
          <Button component={Link} href={`/products/${id}`} variant="default">キャンセル</Button>
        </Group>
        {message && <Alert>{message}</Alert>}
        <SimpleGrid cols={{ base: 1, md: 2 }} spacing="lg">
          <Paper
            component="form"
            onSubmit={(event) => {
              event.preventDefault();
              void updateStock();
            }}
            p="lg"
            withBorder
          >
            <Stack>
              <Title order={2} size="h3">在庫情報を直接更新</Title>
              <Group grow align="start">
                <NumberInput
                  label="現在在庫数" value={quantity} min={0} required disabled={!stock}
                  onChange={(value) => setQuantity(String(value))}
                />
                <NumberInput
                  label="安全在庫数" value={safetyStock} min={0} required disabled={!stock}
                  onChange={(value) => setSafetyStock(String(value))}
                />
              </Group>
              <Textarea
                label="更新理由" placeholder="棚卸調整など" value={stockReason} rows={3}
                onChange={(event) => setStockReason(event.currentTarget.value)}
              />
              <Button type="submit" loading={isSavingStock} disabled={!stock}>在庫情報を更新</Button>
            </Stack>
          </Paper>
          <Paper
            component="form"
            onSubmit={(event) => {
              event.preventDefault();
              void createTransaction();
            }}
            p="lg"
            withBorder
          >
            <Stack>
              <Title order={2} size="h3">在庫取引を登録</Title>
              <Text size="sm">現在在庫数（更新前）: {stock?.quantity ?? "-"}</Text>
              <Select
                label="取引種別" value={String(transactionType)} allowDeselect={false}
                onChange={(value) => setTransactionType(Number(value) as StockTransactionType)}
                data={[
                  { value: "0", label: "入庫" },
                  { value: "1", label: "出庫" },
                  { value: "2", label: "調整" },
                ]}
              />
              <NumberInput
                label="数量" value={transactionQuantity} min={1} required
                onChange={(value) => setTransactionQuantity(String(value))}
              />
              <Text size="sm" c="dimmed">取引後在庫（プレビュー）: {(stock?.quantity ?? 0) + quantityDelta}</Text>
              <Textarea
                label="理由・メモ" value={transactionReason} rows={3}
                onChange={(event) => setTransactionReason(event.currentTarget.value)}
              />
              <Button type="submit" loading={isSavingTransaction} disabled={!stock}>取引を登録</Button>
            </Stack>
          </Paper>
        </SimpleGrid>
        <Paper p="lg" withBorder>
          <Title order={2} size="h3" mb="md">直近の在庫取引履歴</Title>
          <Table.ScrollContainer minWidth={650}>
            <Table striped>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>日時</Table.Th><Table.Th>種別</Table.Th>
                  <Table.Th>増減</Table.Th><Table.Th>取引後在庫</Table.Th>
                  <Table.Th>理由</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {transactions?.items.map((transaction) => (
                  <Table.Tr key={transaction.id}>
                    <Table.Td>{new Date(transaction.createdAt).toLocaleString()}</Table.Td>
                    <Table.Td>{transactionTypeLabels[transaction.type]}</Table.Td>
                    <Table.Td fw={700}>{transaction.quantityDelta}</Table.Td>
                    <Table.Td>{transaction.quantityAfter}</Table.Td>
                    <Table.Td>{transaction.reason ?? "-"}</Table.Td>
                  </Table.Tr>
                ))}
                {(!transactions || transactions.items.length === 0) && (
                  <Table.Tr><Table.Td colSpan={5} ta="center" py="xl">在庫取引履歴がありません。</Table.Td></Table.Tr>
                )}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        </Paper>
      </Stack>
    </Container>
  );
}
