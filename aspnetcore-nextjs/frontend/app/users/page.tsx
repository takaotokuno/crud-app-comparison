"use client";

import {
  Alert, Breadcrumbs, Button, Container, Group, Pagination, Paper, Select,
  Stack, Table, Text, TextInput, Title,
} from "@mantine/core";
import Link from "next/link";
import { useEffect, useState } from "react";
import { requestJson } from "@/lib/api";
import { AccountUser, roleLabels, UserListResponse } from "@/lib/types";

const PAGE_SIZE = 20;

export default function UsersPage() {
  const [users, setUsers] = useState<AccountUser[]>([]);
  const [query, setQuery] = useState("");
  const [role, setRole] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState<string | null>("created_at");
  const [sortDirection, setSortDirection] = useState<string | null>("desc");
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [message, setMessage] = useState("ユーザー一覧を取得中です...");
  const [isLoading, setIsLoading] = useState(true);

  async function loadUsers(nextPage: number) {
    setIsLoading(true);
    setMessage("ユーザー一覧を取得中です...");
    try {
      const searchParams = new URLSearchParams({
        page: String(nextPage),
        page_size: String(PAGE_SIZE),
        sort_by: sortBy ?? "created_at",
        sort_direction: sortDirection ?? "desc",
      });
      if (query.trim()) searchParams.set("q", query.trim());
      if (role) searchParams.set("role", role);
      const data = await requestJson<UserListResponse>(
        `/api/users?${searchParams.toString()}`,
      );
      setUsers(data.items);
      setPage(data.page);
      setTotalCount(data.totalCount);
      setMessage(`${data.totalCount} 件中 ${data.items.length} 件を表示しています。`);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "ユーザー一覧の取得に失敗しました。");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadUsers(1);
  }, []);

  function clearFilters() {
    setQuery("");
    setRole(null);
    setSortBy("created_at");
    setSortDirection("desc");
  }

  return (
    <Container component="main" size="lg" py="xl">
      <Stack gap="lg">
        <Breadcrumbs><span>ユーザー管理</span></Breadcrumbs>
        <Group justify="space-between">
          <Title order={1}>ユーザー管理</Title>
          <Button component={Link} href="/users/new">新規登録</Button>
        </Group>
        <Paper component="form" p="md" withBorder onSubmit={(event) => {
          event.preventDefault();
          void loadUsers(1);
        }}>
          <Stack>
            <Group align="end" grow>
              <TextInput
                label="メールアドレスまたは表示名"
                placeholder="検索キーワード"
                value={query}
                onChange={(event) => setQuery(event.currentTarget.value)}
              />
              <Select
                label="ロール"
                placeholder="すべて"
                value={role}
                onChange={setRole}
                clearable
                data={[
                  { value: "0", label: "管理者" },
                  { value: "1", label: "在庫担当者" },
                  { value: "2", label: "閲覧者" },
                ]}
              />
              <Select
                label="並び替え"
                value={sortBy}
                onChange={setSortBy}
                allowDeselect={false}
                data={[
                  { value: "email", label: "メールアドレス" },
                  { value: "name", label: "表示名" },
                  { value: "role", label: "ロール" },
                  { value: "created_at", label: "作成日時" },
                  { value: "updated_at", label: "更新日時" },
                ]}
              />
              <Select
                label="順序"
                value={sortDirection}
                onChange={setSortDirection}
                allowDeselect={false}
                data={[
                  { value: "asc", label: "昇順" },
                  { value: "desc", label: "降順" },
                ]}
              />
            </Group>
            <Group justify="flex-end">
              <Button type="button" variant="default" onClick={clearFilters}>条件をクリア</Button>
              <Button type="submit" loading={isLoading}>検索</Button>
            </Group>
            <Alert color={isLoading ? "blue" : undefined}>{message}</Alert>
          </Stack>
        </Paper>
        <Table.ScrollContainer minWidth={1000}>
          <Table striped highlightOnHover withTableBorder>
            <Table.Thead>
              <Table.Tr><Table.Th>メールアドレス</Table.Th><Table.Th>表示名</Table.Th><Table.Th>ロール</Table.Th><Table.Th>作成日時</Table.Th><Table.Th>更新日時</Table.Th><Table.Th>操作</Table.Th></Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {users.map((user) => (
                <Table.Tr key={user.id}>
                  <Table.Td>{user.email}</Table.Td>
                  <Table.Td>{user.name}</Table.Td>
                  <Table.Td>{roleLabels[user.role]}</Table.Td>
                  <Table.Td>{new Date(user.createdAt).toLocaleString()}</Table.Td>
                  <Table.Td>{new Date(user.updatedAt).toLocaleString()}</Table.Td>
                  <Table.Td><Button component={Link} href={`/users/${user.id}`} size="xs" variant="default">詳細</Button></Table.Td>
                </Table.Tr>
              ))}
              {!isLoading && users.length === 0 && (
                <Table.Tr><Table.Td colSpan={6} ta="center" py="xl"><Text c="dimmed">ユーザーが登録されていません。</Text></Table.Td></Table.Tr>
              )}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
        {totalCount > PAGE_SIZE && <Pagination value={page} total={Math.ceil(totalCount / PAGE_SIZE)} onChange={loadUsers} mx="auto" />}
      </Stack>
    </Container>
  );
}
