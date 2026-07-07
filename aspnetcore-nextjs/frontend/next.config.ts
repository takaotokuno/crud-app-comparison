import type { NextConfig } from "next";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/login",
        destination: `${apiBaseUrl}/login`,
      },
      {
        source: "/api/logout",
        destination: `${apiBaseUrl}/logout`,
      },
      {
        source: "/api/me",
        destination: `${apiBaseUrl}/me`,
      },
      {
        source: "/api/products/:path*",
        destination: `${apiBaseUrl}/products/:path*`,
      },
      {
        source: "/api/stocks/:path*",
        destination: `${apiBaseUrl}/api/stocks/:path*`,
      },
      {
        source: "/api/stock-transactions/:path*",
        destination: `${apiBaseUrl}/api/stock-transactions/:path*`,
      },
    ];
  },
};

export default nextConfig;
