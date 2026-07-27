export type UserRole = 0 | 1 | 2;
export type ProductStatus = 0 | 1 | 2;
export type StockTransactionType = 0 | 1 | 2;

export type AccountUser = {
  id: string;
  email: string;
  name: string;
  role: UserRole;
};

export type UserListResponse = {
  items: AccountUser[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type UserFormState = {
  email: string;
  name: string;
  role: UserRole;
  password: string;
};

export type ProductSummary = {
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

export type ProductDetail = ProductSummary & {
  description: string | null;
  createdAt: string;
};

export type ProductListResponse = {
  items: ProductSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type ProductFormState = {
  sku: string;
  name: string;
  description: string;
  category: string;
  price: string;
  status: ProductStatus;
  initialQuantity: string;
  safetyStock: string;
};

export const initialFormState: ProductFormState = {
  sku: "",
  name: "",
  description: "",
  category: "",
  price: "",
  status: 0,
  initialQuantity: "0",
  safetyStock: "0",
};

export const statusLabels: Record<ProductStatus, string> = {
  0: "販売中",
  1: "停止中",
  2: "廃番",
};

export const roleLabels: Record<UserRole, string> = {
  0: "管理者",
  1: "在庫担当者",
  2: "閲覧者",
};


export type StockDetail = {
  id: string;
  productId: string;
  productSku: string;
  productName: string;
  quantity: number;
  safetyStock: number;
  isLowStock: boolean;
  createdAt?: string;
  updatedAt: string;
};

export type StockListResponse = {
  items: StockDetail[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type StockTransaction = {
  id: string;
  productId: string;
  stockId: string;
  type: StockTransactionType;
  quantityDelta: number;
  quantityAfter: number;
  reason: string | null;
  createdById: string | null;
  createdAt: string;
};

export type StockTransactionListResponse = {
  items: StockTransaction[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type StockPageResponse = {
  product: ProductDetail;
  stock: StockDetail | null;
  transactions: StockTransactionListResponse;
};

export const transactionTypeLabels: Record<StockTransactionType, string> = {
  0: "入庫",
  1: "出庫",
  2: "調整",
};
