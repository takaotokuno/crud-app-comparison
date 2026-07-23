import type { ReactNode } from "react";
import { requireRole } from "@/lib/auth-server";

export default async function StockProductLayout({ children }: { children: ReactNode }) {
  await requireRole([0, 1]);
  return children;
}
