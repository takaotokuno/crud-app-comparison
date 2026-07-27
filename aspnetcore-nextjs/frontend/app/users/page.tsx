"use client";

import { Alert, Breadcrumbs, Button, Container, Group, Pagination, Stack, Table, Text, Title } from "@mantine/core";
import Link from "next/link";
import { useEffect, useState } from "react";
import { requestJson } from "@/lib/api";
import { AccountUser, roleLabels, UserListResponse } from "@/lib/types";

const PAGE_SIZE = 20;

export default function UsersPage() {
  const [users, setUsers] = useState<AccountUser[]>([]);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [message, setMessage] = useState("ユーザー一覧を取得中です...");
  const [isLoading, setIsLoading] = useState(true);

  async function loadUsers(nextPage: number) {
    setIsLoading(true);
    try {
      const data = await requestJson<UserListResponse>(
        `/api/users?page=${nextPage}&page_size=${PAGE_SIZE}`,
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

  return (
    <Container component="main" size="lg" py="xl">
      <Stack gap="lg">
        <Breadcrumbs><span>ユーザー管理</span></Breadcrumbs>
        <Group justify="space-between">
          <Title order={1}>ユーザー管理</Title>
          <Button component={Link} href="/users/new">新規登録</Button>
        </Group>
        <Alert color={isLoading ? "blue" : undefined}>{message}</Alert>
        <Table.ScrollContainer minWidth={700}>
          <Table striped highlightOnHover withTableBorder>
            <Table.Thead>
              <Table.Tr><Table.Th>メールアドレス</Table.Th><Table.Th>表示名</Table.Th><Table.Th>ロール</Table.Th><Table.Th>操作</Table.Th></Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {users.map((user) => (
                <Table.Tr key={user.id}>
                  <Table.Td>{user.email}</Table.Td>
                  <Table.Td>{user.name}</Table.Td>
                  <Table.Td>{roleLabels[user.role]}</Table.Td>
                  <Table.Td><Button component={Link} href={`/users/${user.id}`} size="xs" variant="default">詳細</Button></Table.Td>
                </Table.Tr>
              ))}
              {!isLoading && users.length === 0 && (
                <Table.Tr><Table.Td colSpan={4} ta="center" py="xl"><Text c="dimmed">ユーザーが登録されていません。</Text></Table.Td></Table.Tr>
              )}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
        {totalCount > PAGE_SIZE && <Pagination value={page} total={Math.ceil(totalCount / PAGE_SIZE)} onChange={loadUsers} mx="auto" />}
      </Stack>
    </Container>
  );
}
