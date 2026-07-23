import type { ReactNode } from "react";
import { requireRole } from "@/lib/auth-server";

export default async function NewProductLayout({ children }: { children: ReactNode }) {
  await requireRole([0]);
  return children;
}
