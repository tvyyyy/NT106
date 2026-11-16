#!/usr/bin/env bash
set -e
cd "$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_PROJ="src/Presentation/BobaShop.WebAPI"
CLIENT_PROJ="src/Presentation/Clients/BobaShop.CustomerApp"
PG_DIR="/c/Users/Admin/Downloads/postgresql-18.7-1-windows-x64-binaries/pgsql"
API_PID=""
trap '[ -n "$API_PID" ] && kill "$API_PID" 2>/dev/null || true' EXIT

PGPASSWORD=123456 "$PG_DIR/bin/createdb.exe" -U postgres bobashop 2>/dev/null || true

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=bobashop;Username=postgres;Password=123456" --project "$API_PROJ" >/dev/null
dotnet user-secrets set "Jwt:Key" "BobaShop_SuperSecret_JWT_Key_NT106_2026_Secure!" --project "$API_PROJ" >/dev/null

echo "Đang khởi chạy WebAPI..."
dotnet run --project "$API_PROJ" --launch-profile http &
API_PID=$!

for i in $(seq 1 30); do
  curl -s http://localhost:5227 >/dev/null 2>&1 && break
  sleep 1
done

echo "Đang mở cửa sổ Đăng nhập..."
dotnet run --project "$CLIENT_PROJ"
