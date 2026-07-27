"use client";

import { Breadcrumbs, Container, Stack, Title } from "@mantine/core";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { UserForm } from "@/components/UserForm";
import { requestJson } from "@/lib/api";
import type { AccountUser, UserFormState } from "@/lib/types";

const initialValue: UserFormState = { email: "", name: "", role: 2, password: "" };

export default function NewUserPage() {
  const router = useRouter();
  async function createUser(form: UserFormState) {
    const user = await requestJson<AccountUser>("/api/users", {
      method: "POST",
      body: JSON.stringify({ email: form.email.trim(), name: form.name.trim(), role: form.role, password: form.password }),
    });
    router.push(`/users/${user.id}`);
  }
  return (
    <Container component="main" size="sm" py="xl"><Stack>
      <Breadcrumbs><Link href="/users">ユーザー管理</Link><span>新規登録</span></Breadcrumbs>
      <Title order={1}>ユーザー登録</Title>
      <UserForm initialValue={initialValue} submitLabel="登録する" cancelHref="/users" requirePassword onSubmit={createUser} />
    </Stack></Container>
  );
}
