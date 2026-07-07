"use client";

import { FormEvent, useMemo, useState } from "react";

type ProductStatus = 0 | 1 | 2;

type ProductSummary = {
  id: string;
  sku: string;
  name: string;
  category: string | null;
  price: number;
  status: ProductStatus;
  quantity: number;
  safetyStock: number;
  updatedAt: string;
};

type ProductDetail = ProductSummary & {
  description: string | null;
  createdAt: string;
};

type ProductListResponse = {
  items: ProductSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
};

type ProductFormState = {
  sku: string;
  name: string;
  description: string;
  category: string;
  price: string;
  status: ProductStatus;
  initialQuantity: string;
  safetyStock: string;
};

const initialFormState: ProductFormState = {
  sku: "",
  name: "",
  description: "",
  category: "",
  price: "",
  status: 0,
  initialQuantity: "0",
  safetyStock: "0",
};

const statusLabels: Record<ProductStatus, string> = {
  0: "Active",
  1: "Inactive",
  2: "Discontinued",
};

const apiBaseUrl = "";

async function requestJson<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers,
    },
  });

  if (!response.ok) {
    const body = (await response.json().catch(() => undefined)) as
      | { message?: string }
      | undefined;
    throw new Error(body?.message ?? `API request failed: ${response.status}`);
  }

  return response.json() as Promise<T>;
}

function toOptionalValue(value: string) {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

export default function Home() {
  const [products, setProducts] = useState<ProductSummary[]>([]);
  const [selectedProduct, setSelectedProduct] = useState<ProductDetail | null>(null);
  const [form, setForm] = useState<ProductFormState>(initialFormState);
  const [query, setQuery] = useState("");
  const [message, setMessage] = useState("商品一覧を取得してください。");
  const [isLoading, setIsLoading] = useState(false);

  const totalStock = useMemo(
    () => products.reduce((total, product) => total + product.quantity, 0),
    [products],
  );

  async function loadProducts() {
    setIsLoading(true);
    setMessage("商品一覧を取得中です...");

    try {
      const searchParams = new URLSearchParams({ page: "1", page_size: "20" });
      if (query.trim()) {
        searchParams.set("q", query.trim());
      }

      const data = await requestJson<ProductListResponse>(
        `/api/products?${searchParams.toString()}`,
      );
      setProducts(data.items);
      setMessage(`${data.totalCount} 件中 ${data.items.length} 件の商品を取得しました。`);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "商品一覧の取得に失敗しました。");
    } finally {
      setIsLoading(false);
    }
  }

  async function loadProductDetail(id: string) {
    setIsLoading(true);
    setMessage("商品詳細を取得中です...");

    try {
      const product = await requestJson<ProductDetail>(`/api/products/${id}`);
      setSelectedProduct(product);
      setMessage(`${product.name} の詳細を取得しました。`);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "商品詳細の取得に失敗しました。");
    } finally {
      setIsLoading(false);
    }
  }

  async function createProduct(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsLoading(true);
    setMessage("商品を保存中です...");

    try {
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

      setSelectedProduct(product);
      setForm(initialFormState);
      setMessage(`${product.name} を保存しました。`);
      await loadProducts();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "商品の保存に失敗しました。");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-6xl flex-col gap-8 px-6 py-10 text-slate-900">
      <header className="space-y-2 border-b border-slate-200 pb-6">
        <p className="text-sm text-slate-500">ASP.NET Core Web API + Next.js</p>
        <h1 className="text-3xl font-semibold">商品レコード管理</h1>
        <p className="text-slate-600">
          ログインなしのプレーンな画面です。商品APIの取得・詳細取得・新規保存だけを呼び出します。
        </p>
      </header>

      <section className="rounded border border-slate-200 p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
          <label className="flex flex-1 flex-col gap-1 text-sm font-medium">
            検索キーワード
            <input
              className="rounded border border-slate-300 px-3 py-2 font-normal"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="SKU / 商品名など"
            />
          </label>
          <button
            className="rounded bg-slate-900 px-4 py-2 text-white disabled:opacity-50"
            onClick={loadProducts}
            disabled={isLoading}
          >
            商品一覧を取得
          </button>
        </div>
        <p className="mt-3 text-sm text-slate-600">{message}</p>
      </section>

      <div className="grid gap-8 lg:grid-cols-[1fr_360px]">
        <section className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">商品一覧</h2>
            <p className="text-sm text-slate-500">
              表示数: {products.length} / 在庫合計: {totalStock}
            </p>
          </div>
          <div className="overflow-x-auto rounded border border-slate-200">
            <table className="min-w-full border-collapse text-left text-sm">
              <thead className="bg-slate-50">
                <tr>
                  <th className="border-b border-slate-200 px-3 py-2">SKU</th>
                  <th className="border-b border-slate-200 px-3 py-2">商品名</th>
                  <th className="border-b border-slate-200 px-3 py-2">カテゴリ</th>
                  <th className="border-b border-slate-200 px-3 py-2">価格</th>
                  <th className="border-b border-slate-200 px-3 py-2">在庫</th>
                  <th className="border-b border-slate-200 px-3 py-2">状態</th>
                  <th className="border-b border-slate-200 px-3 py-2">操作</th>
                </tr>
              </thead>
              <tbody>
                {products.map((product) => (
                  <tr key={product.id}>
                    <td className="border-b border-slate-100 px-3 py-2 font-mono">{product.sku}</td>
                    <td className="border-b border-slate-100 px-3 py-2">{product.name}</td>
                    <td className="border-b border-slate-100 px-3 py-2">{product.category ?? "-"}</td>
                    <td className="border-b border-slate-100 px-3 py-2">
                      ¥{product.price.toLocaleString()}
                    </td>
                    <td className="border-b border-slate-100 px-3 py-2">{product.quantity}</td>
                    <td className="border-b border-slate-100 px-3 py-2">
                      {statusLabels[product.status]}
                    </td>
                    <td className="border-b border-slate-100 px-3 py-2">
                      <button
                        className="rounded border border-slate-300 px-2 py-1"
                        onClick={() => loadProductDetail(product.id)}
                        disabled={isLoading}
                      >
                        詳細
                      </button>
                    </td>
                  </tr>
                ))}
                {products.length === 0 && (
                  <tr>
                    <td className="px-3 py-8 text-center text-slate-500" colSpan={7}>
                      商品一覧は未取得です。
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </section>

        <aside className="space-y-6">
          <section className="rounded border border-slate-200 p-4">
            <h2 className="text-xl font-semibold">新規保存</h2>
            <form className="mt-4 space-y-3" onSubmit={createProduct}>
              <input
                className="w-full rounded border border-slate-300 px-3 py-2"
                value={form.sku}
                onChange={(event) => setForm({ ...form, sku: event.target.value })}
                placeholder="SKU"
                required
              />
              <input
                className="w-full rounded border border-slate-300 px-3 py-2"
                value={form.name}
                onChange={(event) => setForm({ ...form, name: event.target.value })}
                placeholder="商品名"
                required
              />
              <textarea
                className="w-full rounded border border-slate-300 px-3 py-2"
                value={form.description}
                onChange={(event) => setForm({ ...form, description: event.target.value })}
                placeholder="説明"
                rows={3}
              />
              <input
                className="w-full rounded border border-slate-300 px-3 py-2"
                value={form.category}
                onChange={(event) => setForm({ ...form, category: event.target.value })}
                placeholder="カテゴリ"
              />
              <input
                className="w-full rounded border border-slate-300 px-3 py-2"
                value={form.price}
                onChange={(event) => setForm({ ...form, price: event.target.value })}
                placeholder="価格"
                type="number"
                min="0"
                required
              />
              <div className="grid grid-cols-2 gap-3">
                <input
                  className="rounded border border-slate-300 px-3 py-2"
                  value={form.initialQuantity}
                  onChange={(event) => setForm({ ...form, initialQuantity: event.target.value })}
                  placeholder="初期在庫"
                  type="number"
                  min="0"
                  required
                />
                <input
                  className="rounded border border-slate-300 px-3 py-2"
                  value={form.safetyStock}
                  onChange={(event) => setForm({ ...form, safetyStock: event.target.value })}
                  placeholder="安全在庫"
                  type="number"
                  min="0"
                  required
                />
              </div>
              <select
                className="w-full rounded border border-slate-300 px-3 py-2"
                value={form.status}
                onChange={(event) =>
                  setForm({ ...form, status: Number(event.target.value) as ProductStatus })
                }
              >
                <option value={0}>Active</option>
                <option value={1}>Inactive</option>
                <option value={2}>Discontinued</option>
              </select>
              <button
                className="w-full rounded bg-slate-900 px-4 py-2 text-white disabled:opacity-50"
                type="submit"
                disabled={isLoading}
              >
                保存
              </button>
            </form>
          </section>

          <section className="rounded border border-slate-200 p-4">
            <h2 className="text-xl font-semibold">選択中の商品</h2>
            {selectedProduct ? (
              <dl className="mt-4 space-y-2 text-sm">
                <div>
                  <dt className="font-medium">ID</dt>
                  <dd className="break-all font-mono text-slate-600">{selectedProduct.id}</dd>
                </div>
                <div>
                  <dt className="font-medium">商品名</dt>
                  <dd>{selectedProduct.name}</dd>
                </div>
                <div>
                  <dt className="font-medium">説明</dt>
                  <dd>{selectedProduct.description ?? "-"}</dd>
                </div>
                <div>
                  <dt className="font-medium">更新日時</dt>
                  <dd>{new Date(selectedProduct.updatedAt).toLocaleString()}</dd>
                </div>
              </dl>
            ) : (
              <p className="mt-4 text-sm text-slate-500">一覧から詳細を押すと表示します。</p>
            )}
          </section>
        </aside>
      </div>
    </main>
  );
}
