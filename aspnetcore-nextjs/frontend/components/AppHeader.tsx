"use client";

import { Alert, Box, Button, Container, Group, Stack, Text, Title } from "@mantine/core";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useCurrentUser } from "@/components/AuthProvider";
import { requestJson } from "@/lib/api";
import { roleLabels } from "@/lib/types";

export function AppHeader() {
  const router = useRouter();
  const user = useCurrentUser();
  const [message, setMessage] = useState("");

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
            {user.role === 0 && (
              <Button component={Link} href="/products/new">新規登録</Button>
            )}
            {user.role === 0 && (
              <Button component={Link} href="/users" variant="default">ユーザー管理</Button>
            )}
            <Text size="sm">{roleLabels[user.role]}／{user.name}</Text>
            <Button variant="default" onClick={logout}>ログアウト</Button>
          </Group>
        </Group>
        {message && <Alert color="red" mt="sm">{message}</Alert>}
      </Container>
    </Box>
  );
}
