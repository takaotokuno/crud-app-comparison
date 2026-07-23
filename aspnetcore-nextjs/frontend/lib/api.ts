export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

type RequestJsonInit = RequestInit & {
  redirectOnUnauthorized?: boolean;
  redirectOnForbidden?: boolean;
};

const defaultErrorMessages: Partial<Record<number, string>> = {
  401: "ログインが必要です。",
  403: "この操作を行う権限がありません。",
};

export async function requestJson<T>(path: string, init?: RequestJsonInit): Promise<T> {
  const {
    redirectOnUnauthorized = true,
    redirectOnForbidden = true,
    ...requestInit
  } = init ?? {};
  const response = await fetch(path, {
    ...requestInit,
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...requestInit.headers,
    },
  });

  if (!response.ok) {
    const body = (await response.json().catch(() => undefined)) as
      | { message?: string }
      | undefined;
    if (response.status === 401 && redirectOnUnauthorized && typeof window !== "undefined") {
      const returnTo = `${window.location.pathname}${window.location.search}`;
      window.location.assign(`/login?returnTo=${encodeURIComponent(returnTo)}`);
    }

    if (response.status === 403 && redirectOnForbidden && typeof window !== "undefined") {
      window.location.assign("/forbidden");
    }

    throw new ApiError(
      response.status,
      body?.message ?? defaultErrorMessages[response.status] ?? `API request failed: ${response.status}`,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export function toOptionalValue(value: string) {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}
