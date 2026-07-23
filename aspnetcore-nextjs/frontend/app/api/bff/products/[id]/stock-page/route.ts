import { NextRequest, NextResponse } from "next/server";
import type {
  ProductDetail,
  StockListResponse,
  StockPageResponse,
  StockTransactionListResponse,
} from "@/lib/types";

const apiBaseUrl = process.env.API_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

type RouteContext = {
  params: Promise<{ id: string }>;
};

function backendRequest(request: NextRequest, path: string) {
  const headers = new Headers({ Accept: "application/json" });
  const cookie = request.headers.get("cookie");
  if (cookie) headers.set("Cookie", cookie);

  return fetch(new URL(path, apiBaseUrl), {
    cache: "no-store",
    headers,
    signal: request.signal,
  });
}

async function backendError(response: Response) {
  const body = await response.text();
  return new NextResponse(body || null, {
    status: response.status,
    headers: response.headers.get("content-type")
      ? { "Content-Type": response.headers.get("content-type")! }
      : undefined,
  });
}

export async function GET(request: NextRequest, context: RouteContext) {
  const { id } = await context.params;
  const productId = encodeURIComponent(id);

  const [productResponse, stockResponse, transactionsResponse] = await Promise.all([
    backendRequest(request, `/products/${productId}`),
    backendRequest(request, `/api/stocks?product_id=${productId}&page=1&page_size=1`),
    backendRequest(
      request,
      `/api/stock-transactions?product_id=${productId}&page=1&page_size=20`,
    ),
  ]);

  const failedResponse = [productResponse, stockResponse, transactionsResponse].find(
    (response) => !response.ok,
  );
  if (failedResponse) return backendError(failedResponse);

  const [product, stocks, transactions] = (await Promise.all([
    productResponse.json(),
    stockResponse.json(),
    transactionsResponse.json(),
  ])) as [ProductDetail, StockListResponse, StockTransactionListResponse];

  return NextResponse.json<StockPageResponse>({
    product,
    stock: stocks.items[0] ?? null,
    transactions,
  });
}
