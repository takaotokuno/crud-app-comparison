"use client";

import { Alert, Button, Group, Paper, PasswordInput, Select, Stack, TextInput } from "@mantine/core";
import Link from "next/link";
import { FormEvent, useState } from "react";
import type { UserFormState, UserRole } from "@/lib/types";

const roleOptions = [
  { value: "0", label: "管理者" },
  { value: "1", label: "在庫担当者" },
  { value: "2", label: "閲覧者" },
];

export function UserForm({
  initialValue,
  submitLabel,
  cancelHref,
  requirePassword = false,
  onSubmit,
}: {
  initialValue: UserFormState;
  submitLabel: string;
  cancelHref: string;
  requirePassword?: boolean;
  onSubmit: (form: UserFormState) => Promise<void>;
}) {
  const [form, setForm] = useState(initialValue);
  const [message, setMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage("");
    if (!form.email.trim() || !form.name.trim()) {
      setMessage("メールアドレスと表示名を入力してください。");
      return;
    }
    if ((requirePassword || form.password.length > 0) && form.password.length < 8) {
      setMessage("パスワードは8文字以上で入力してください。");
      return;
    }
    setIsSubmitting(true);
    try {
      await onSubmit(form);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "ユーザーの保存に失敗しました。");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <Paper component="form" p="lg" withBorder onSubmit={handleSubmit}>
      <Stack>
        {message && <Alert color="red">{message}</Alert>}
        <TextInput
          required
          type="email"
          label="メールアドレス"
          maxLength={255}
          value={form.email}
          onChange={(event) => setForm({ ...form, email: event.currentTarget.value })}
        />
        <TextInput
          required
          label="表示名"
          maxLength={100}
          value={form.name}
          onChange={(event) => setForm({ ...form, name: event.currentTarget.value })}
        />
        <Select
          required
          label="ロール"
          data={roleOptions}
          value={String(form.role)}
          allowDeselect={false}
          onChange={(value) => setForm({ ...form, role: Number(value) as UserRole })}
        />
        <PasswordInput
          required={requirePassword}
          label="パスワード"
          description={requirePassword ? "8文字以上で入力してください。" : "変更する場合のみ、8文字以上で入力してください。"}
          minLength={requirePassword ? 8 : undefined}
          maxLength={200}
          value={form.password}
          onChange={(event) => setForm({ ...form, password: event.currentTarget.value })}
        />
        <Group justify="flex-end">
          <Button component={Link} href={cancelHref} variant="default">キャンセル</Button>
          <Button type="submit" loading={isSubmitting}>{submitLabel}</Button>
        </Group>
      </Stack>
    </Paper>
  );
}
