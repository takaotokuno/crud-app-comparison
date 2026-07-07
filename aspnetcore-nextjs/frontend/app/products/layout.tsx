import type { ReactNode } from "react";
import { AppHeader } from "@/components/AppHeader";

export default function ProductsLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <AppHeader />
      {children}
    </>
  );
}
