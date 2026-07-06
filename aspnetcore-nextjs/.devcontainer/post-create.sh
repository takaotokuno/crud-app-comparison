#!/usr/bin/env bash
set -euo pipefail

echo "== Restore .NET local tools if manifest exists =="

if [ -f ".config/dotnet-tools.json" ]; then
  dotnet tool restore
else
  echo "No .NET tool manifest found. Installing dotnet-ef globally..."

  if dotnet tool list --global | grep -q '^dotnet-ef '; then
    dotnet tool update --global dotnet-ef --version 9.*
  else
    dotnet tool install --global dotnet-ef --version 9.*
  fi
fi

echo "== .NET info =="
dotnet --info

echo "== dotnet ef version =="
dotnet ef --version

echo "== Node.js version =="
node --version

echo "== npm version =="
npm --version