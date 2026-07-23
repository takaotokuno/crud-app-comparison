import "server-only";

import { headers } from "next/headers";
import { redirect } from "next/navigation";
import { env } from "@/env";
import type { AccountUser, UserRole } from "@/lib/types";

export async function getCurrentUser(): Promise<AccountUser | null> {
  const requestHeaders = await headers();
  const cookie = requestHeaders.get("cookie");
  const response = await fetch(new URL("/me", env.API_BASE_URL), {
    cache: "no-store",
    headers: cookie ? { Cookie: cookie } : undefined,
  });

  if (response.status === 401) return null;
  if (!response.ok) throw new Error(`Authentication API request failed: ${response.status}`);

  return response.json() as Promise<AccountUser>;
}

export async function requireCurrentUser(): Promise<AccountUser> {
  const user = await getCurrentUser();
  if (!user) redirect("/login");
  return user;
}

export async function requireRole(allowedRoles: readonly UserRole[]): Promise<AccountUser> {
  const user = await requireCurrentUser();
  if (!allowedRoles.includes(user.role)) redirect("/forbidden");
  return user;
}
