"use client";

import { createContext, ReactNode, useContext } from "react";
import type { AccountUser, UserRole } from "@/lib/types";

const AuthContext = createContext<AccountUser | null>(null);

export function AuthProvider({ user, children }: { user: AccountUser; children: ReactNode }) {
  return <AuthContext.Provider value={user}>{children}</AuthContext.Provider>;
}

export function useCurrentUser() {
  const user = useContext(AuthContext);
  if (!user) throw new Error("useCurrentUser must be used within AuthProvider");
  return user;
}

export function useHasRole(...roles: UserRole[]) {
  return roles.includes(useCurrentUser().role);
}
