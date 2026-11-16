#!/usr/bin/env bash
set -e

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

API_PROJ="src/Presentation/BobaShop.WebAPI"
CLIENT_PROJ="src/Presentation/Clients/BobaShop.CustomerApp"
API_PID=""

cleanup() {
  if [ -n "$API_PID" ]; then
    echo "Đang tắt BobaShop.WebAPI (PID: $API_PID)..."
    kill "$API_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT

echo "=========================================="
echo "   KHỞI ĐỘNG DEMO BOBASHOP (NT106)"
echo "=========================================="

# Kiểm tra xem PostgreSQL (cổng 5432) có đang chạy không
if netstat -an 2>/dev/null | grep -q ":5432 "; then
  echo "[1/2] Phát hiện PostgreSQL đang chạy trên cổng 5432."
  
  # Tự động cấu hình Jwt:Key nếu chưa có trong User Secrets
  if ! dotnet user-secrets list --project "$API_PROJ" 2>/dev/null | grep -q "Jwt:Key"; then
    echo "      Đang thiết lập Jwt:Key mẫu cho môi trường Development..."
    dotnet user-secrets set "Jwt:Key" "BobaShop_SuperSecret_DemoKey_NT106_2026_Secure!" --project "$API_PROJ" >/dev/null
  fi

  echo "      Đang khởi chạy BobaShop.WebAPI tại http://localhost:5227 ..."
  dotnet run --project "$API_PROJ" --launch-profile http > /tmp/bobashop-api.log 2>&1 &
  API_PID=$!
  sleep 3
  echo "      Swagger API: http://localhost:5227/swagger"
else
  echo "[1/2] Máy chưa bật PostgreSQL (cổng 5432) -> Bỏ qua WebAPI, chạy chế độ Demo độc lập trên Client."
fi

echo "[2/2] Đang build và mở ứng dụng BobaShop.CustomerApp..."
dotnet run --project "$CLIENT_PROJ"
