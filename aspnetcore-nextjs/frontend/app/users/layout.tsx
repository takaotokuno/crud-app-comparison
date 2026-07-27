import type { ReactNode } from "react";
import { AppHeader } from "@/components/AppHeader";
import { AuthProvider } from "@/components/AuthProvider";
import { requireRole } from "@/lib/auth-server";

export default async function UsersLayout({ children }: { children: ReactNode }) {
  const user = await requireRole([0]);

  return (
    <AuthProvider user={user}>
      <AppHeader />
      {children}
    </AuthProvider>
  );
}
