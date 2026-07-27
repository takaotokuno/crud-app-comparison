"use client";

import { Alert, Breadcrumbs, Button, Container, Group, Paper, SimpleGrid, Stack, Text, Title } from "@mantine/core";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useCurrentUser } from "@/components/AuthProvider";
import { requestJson } from "@/lib/api";
import { AccountUser, roleLabels } from "@/lib/types";

export default function UserDetailPage() {
  const currentUser = useCurrentUser();
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [user, setUser] = useState<AccountUser | null>(null);
  const [message, setMessage] = useState("ユーザー詳細を取得中です...");

  useEffect(() => {
    requestJson<AccountUser>(`/api/users/${id}`)
      .then((response) => { setUser(response); setMessage(""); })
      .catch((error) => setMessage(error instanceof Error ? error.message : "ユーザー詳細の取得に失敗しました。"));
  }, [id]);

  async function deleteUser() {
    if (!confirm("このユーザーを削除しますか？この操作は取り消せません。")) return;
    try {
      await requestJson<void>(`/api/users/${id}`, { method: "DELETE" });
      router.push("/users");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "ユーザーの削除に失敗しました。");
    }
  }

  return (
    <Container component="main" size="md" py="xl"><Stack gap="lg">
      <Breadcrumbs><Link href="/users">ユーザー管理</Link><span>ユーザー詳細</span></Breadcrumbs>
      <Group justify="space-between"><Title order={1}>ユーザー詳細</Title><Group>
        <Button component={Link} href="/users" variant="default">一覧へ</Button>
        {user && <Button component={Link} href={`/users/${id}/edit`} variant="default">編集</Button>}
        <Button color="red" disabled={!user || currentUser.id === id} onClick={deleteUser}>削除</Button>
      </Group></Group>
      {currentUser.id === id && <Alert color="blue">ログイン中のユーザーは削除できません。</Alert>}
      {message && <Alert>{message}</Alert>}
      {user && <Paper p="lg" withBorder><SimpleGrid cols={{ base: 1, sm: 2 }} spacing="lg">
        <Info label="メールアドレス" value={user.email} />
        <Info label="表示名" value={user.name} />
        <Info label="ロール" value={roleLabels[user.role]} />
        <Info label="ユーザーID" value={user.id} mono />
        <Info label="作成日時" value={new Date(user.createdAt).toLocaleString()} />
        <Info label="更新日時" value={new Date(user.updatedAt).toLocaleString()} />
      </SimpleGrid></Paper>}
    </Stack></Container>
  );
}

function Info({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return <div><Text size="sm" c="dimmed" fw={500}>{label}</Text><Text ff={mono ? "monospace" : undefined}>{value}</Text></div>;
}
