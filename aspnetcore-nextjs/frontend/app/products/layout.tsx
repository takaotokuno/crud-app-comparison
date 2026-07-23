import type { ReactNode } from "react";
import { AppHeader } from "@/components/AppHeader";
import { AuthProvider } from "@/components/AuthProvider";
import { requireCurrentUser } from "@/lib/auth-server";

export default async function ProductsLayout({ children }: { children: ReactNode }) {
  const user = await requireCurrentUser();

  return (
    <AuthProvider user={user}>
      <AppHeader />
      {children}
    </AuthProvider>
  );
}
