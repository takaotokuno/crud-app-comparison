import { Button, Center, Paper, Stack, Text, Title } from "@mantine/core";
import Link from "next/link";

export default function ForbiddenPage() {
  return (
    <Center component="main" mih="100vh" p="md">
      <Paper w="100%" maw={520} p="xl" withBorder shadow="sm">
        <Stack>
          <Title order={1}>アクセス権限がありません</Title>
          <Text>このページを表示する権限がありません。</Text>
          <Button component={Link} href="/products">商品一覧へ戻る</Button>
        </Stack>
      </Paper>
    </Center>
  );
}
