"use client";

import { Alert, Button, Center, Paper, PasswordInput, Stack, Text, TextInput, Title } from "@mantine/core";
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
      await requestJson<AccountUser>("/api/login", {
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
    <Center component="main" mih="100vh" p="md">
      <Paper component="form" onSubmit={login} w="100%" maw={420} p="xl" withBorder shadow="sm">
        <Stack>
          <div>
            <Text c="dimmed" size="sm">商品在庫管理</Text>
            <Title order={1}>ログイン</Title>
          </div>
          {error && <Alert color="red">{error}</Alert>}
          <TextInput
            label="メールアドレス"
            value={email}
            onChange={(event) => setEmail(event.currentTarget.value)}
            type="email"
            required
          />
          <PasswordInput
            label="パスワード"
            value={password}
            onChange={(event) => setPassword(event.currentTarget.value)}
            required
          />
          <Button loading={isLoading} type="submit" fullWidth>ログイン</Button>
        </Stack>
      </Paper>
    </Center>
  );
}
