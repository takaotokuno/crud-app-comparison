import type { NextConfig } from "next";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/products/:path*",
        destination: `${apiBaseUrl}/api/products/:path*`,
      },
    ];
  },
};

export default nextConfig;
