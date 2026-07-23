import { z } from "zod";

const envSchema = z.object({
  API_BASE_URL: z
    .string()
    .url()
    .refine((value) => ["http:", "https:"].includes(new URL(value).protocol), {
      message: "API_BASE_URL must use the http or https protocol",
    }),
});

export const env = envSchema.parse({
  API_BASE_URL: process.env.API_BASE_URL,
});
