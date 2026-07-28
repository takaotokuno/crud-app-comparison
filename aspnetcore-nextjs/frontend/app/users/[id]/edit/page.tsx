"use client";

import { Alert, Breadcrumbs, Container, Stack, Title } from "@mantine/core";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { UserForm } from "@/components/UserForm";
import { requestJson } from "@/lib/api";
import type { AccountUser, UserFormState } from "@/lib/types";

export default function EditUserPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [user, setUser] = useState<AccountUser | null>(null);
  const [message, setMessage] = useState("ユーザー情報を取得中です...");

  useEffect(() => {
    requestJson<AccountUser>(`/api/users/${id}`)
      .then((response) => { setUser(response); setMessage(""); })
      .catch((error) => setMessage(error instanceof Error ? error.message : "ユーザー情報の取得に失敗しました。"));
  }, [id]);

  async function updateUser(form: UserFormState) {
    const password = form.password;
    await requestJson<AccountUser>(`/api/users/${id}`, {
      method: "PUT",
      body: JSON.stringify({
        email: form.email.trim(),
        name: form.name.trim(),
        role: form.role,
        ...(password ? { password } : {}),
      }),
    });
    router.push(`/users/${id}`);
  }

  return (
    <Container component="main" size="sm" py="xl"><Stack>
      <Breadcrumbs><Link href="/users">ユーザー管理</Link><Link href={`/users/${id}`}>ユーザー詳細</Link><span>編集</span></Breadcrumbs>
      <Title order={1}>ユーザー編集</Title>
      {message && <Alert>{message}</Alert>}
      {user && <UserForm key={user.id} initialValue={{ email: user.email, name: user.name, role: user.role, password: "" }} submitLabel="更新する" cancelHref={`/users/${id}`} onSubmit={updateUser} />}
    </Stack></Container>
  );
}
