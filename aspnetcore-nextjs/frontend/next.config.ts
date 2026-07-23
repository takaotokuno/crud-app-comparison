import type { NextConfig } from "next";
import { env } from "./env";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/login",
        destination: `${env.API_BASE_URL}/login`,
      },
      {
        source: "/api/logout",
        destination: `${env.API_BASE_URL}/logout`,
      },
      {
        source: "/api/me",
        destination: `${env.API_BASE_URL}/me`,
      },
      {
        source: "/api/products/:path*",
        destination: `${env.API_BASE_URL}/products/:path*`,
      },
      {
        source: "/api/stocks/:path*",
        destination: `${env.API_BASE_URL}/api/stocks/:path*`,
      },
      {
        source: "/api/stock-transactions/:path*",
        destination: `${env.API_BASE_URL}/api/stock-transactions/:path*`,
      },
    ];
  },
};

export default nextConfig;
