"use client";

import { Alert, Box, Button, Container, Group, Stack, Text, Title } from "@mantine/core";
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
    requestJson<AccountUser>("/api/me").then(setUser).catch(() => setUser(null));
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
    <Box component="header" bg="white" style={{ borderBottom: "1px solid var(--mantine-color-gray-3)" }}>
      <Container size="lg" py="md">
        <Group justify="space-between" align="center" wrap="wrap">
          <Stack gap={0}>
            <Text c="dimmed" size="xs">ASP.NET Core Web API + Next.js</Text>
            <Title component={Link} href="/products" order={2} td="none" c="dark">
              商品在庫管理
            </Title>
          </Stack>
          <Group component="nav" gap="sm" wrap="wrap">
            <Button component={Link} href="/products" variant="default">商品一覧</Button>
            {user?.role !== 2 && (
              <Button component={Link} href="/products/new">新規登録</Button>
            )}
            {user?.role === 0 && (
              <Button variant="default" disabled>ユーザー管理（準備中）</Button>
            )}
            {user ? (
              <>
                <Text size="sm">{roleLabels[user.role]}／{user.name}</Text>
                <Button variant="default" onClick={logout}>ログアウト</Button>
              </>
            ) : (
              <Button component={Link} href="/login" variant="default">ログイン</Button>
            )}
          </Group>
        </Group>
        {message && <Alert color="red" mt="sm">{message}</Alert>}
      </Container>
    </Box>
  );
}
