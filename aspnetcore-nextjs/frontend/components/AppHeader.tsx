"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { requestJson } from "@/lib/api";
import { AccountUser, roleLabels } from "@/lib/types";

export function AppHeader() {
  const router = useRouter();
  const [user, setUser] = useState<AccountUser | null>(null);
  const [message, setMessage] = useState("");

  useEffect(() => {
    requestJson<AccountUser>("/api/me")
      .then(setUser)
      .catch(() => setUser(null));
  }, []);

  async function logout() {
    try {
      await requestJson<void>("/api/logout", { method: "POST" });
      router.push("/login");
      router.refresh();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "ログアウトに失敗しました。");
    }
  }

  return (
    <header className="border-b border-slate-200 bg-white">
      <div className="mx-auto flex w-full max-w-6xl flex-col gap-4 px-6 py-5 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-sm text-slate-500">ASP.NET Core Web API + Next.js</p>
          <Link className="text-2xl font-semibold text-slate-950" href="/products">
            商品在庫管理
          </Link>
        </div>
        <nav className="flex flex-wrap items-center gap-3 text-sm">
          <Link className="rounded border border-slate-300 px-3 py-2" href="/products">
            商品一覧
          </Link>
          <Link className="rounded bg-slate-900 px-3 py-2 text-white" href="/products/new">
            新規登録
          </Link>
          {user ? (
            <>
              <span className="text-slate-600">
                {user.name}（{roleLabels[user.role]}）
              </span>
              <button className="rounded border border-slate-300 px-3 py-2" onClick={logout}>
                ログアウト
              </button>
            </>
          ) : (
            <Link className="rounded border border-slate-300 px-3 py-2" href="/login">
              ログイン
            </Link>
          )}
        </nav>
      </div>
      {message && <p className="mx-auto max-w-6xl px-6 pb-3 text-sm text-red-600">{message}</p>}
    </header>
  );
}
