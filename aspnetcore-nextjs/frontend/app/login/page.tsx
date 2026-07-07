"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { requestJson } from "@/lib/api";
import { AccountUser } from "@/lib/types";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("admin@example.com");
  const [password, setPassword] = useState("password");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  async function login(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setIsLoading(true);
    try {
      await requestJson<AccountUser>("/api/account/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });
      router.push("/products");
      router.refresh();
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "ログインに失敗しました。");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-md flex-col justify-center px-6 py-10 text-slate-900">
      <form className="space-y-5 rounded border border-slate-200 bg-white p-6 shadow-sm" onSubmit={login}>
        <div>
          <p className="text-sm text-slate-500">商品在庫管理</p>
          <h1 className="text-2xl font-semibold">ログイン</h1>
        </div>
        {error && <p className="rounded bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}
        <label className="block text-sm font-medium">メールアドレス<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={email} onChange={(event) => setEmail(event.target.value)} type="email" required /></label>
        <label className="block text-sm font-medium">パスワード<input className="mt-1 w-full rounded border border-slate-300 px-3 py-2 font-normal" value={password} onChange={(event) => setPassword(event.target.value)} type="password" required /></label>
        <button className="w-full rounded bg-slate-900 px-4 py-2 text-white disabled:opacity-50" disabled={isLoading} type="submit">ログイン</button>
      </form>
    </main>
  );
}
