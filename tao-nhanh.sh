#!/usr/bin/env bash
# Tạo / sửa nhánh theo "Quy chuẩn phát triển phần mềm" (mục 8 và 10), repo NT106.
# Chạy trong thư mục gốc repo.  bash tao-nhanh.sh [--dry]
# An toàn khi chạy lại: nhánh đã có thì bỏ qua.
set -uo pipefail

DRY=0; [ "${1:-}" = "--dry" ] && DRY=1
run() { if [ $DRY -eq 1 ]; then echo "[dry] $*"; else "$@"; fi; }
exists() { git ls-remote --exit-code --heads origin "$1" >/dev/null 2>&1; }

BASE=develop
run git fetch origin

# 1) develop: quy chuẩn bắt buộc tạo nhánh từ develop
if exists "$BASE"; then echo "develop đã có"; else
  run git branch -f "$BASE" origin/main
  run git push -u origin "$BASE"
fi

# 2) Đổi tên feature/test-auth (scope "test" không có trong quy chuẩn) -> feature/scripts-test-auth
#    Chỉ xóa khi nhánh cũ còn trống (cùng commit với develop), tránh mất việc.
OLD=feature/test-auth
if exists "$OLD"; then
  oldsha=$(git ls-remote --heads origin "$OLD" | cut -f1)
  basesha=$(git ls-remote --heads origin "$BASE" | cut -f1)
  if [ "$oldsha" = "$basesha" ]; then
    run git push origin --delete "$OLD"
    run git branch -D "$OLD" 2>/dev/null || true
  else
    echo "CẢNH BÁO: $OLD đã có commit, không xóa. Tự đổi tên bằng tay."
  fi
fi

BRANCHES=(
  # Vy: webapi, infrastructure, persistence (+ application, domain do Owner Core là Vy)
  feature/persistence-password-reset-token
  feature/persistence-seed
  feature/infrastructure-auth
  feature/infrastructure-email
  feature/application-auth
  feature/application-menu
  feature/webapi-auth
  feature/webapi-menu
  feature/webapi-firebase-demo

  # Ngân: customerapp, adminapp, client-network (giữa kỳ Tài làm thay)
  feature/client-network-api-client
  feature/client-network-theme
  feature/customerapp-auth
  feature/customerapp-home
  feature/adminapp-auth

  # Tài: voucher, review, test
  feature/webapi-voucher
  feature/customerapp-voucher
  feature/adminapp-voucher
  feature/webapi-review
  feature/customerapp-review
  feature/scripts-test-auth
)

for b in "${BRANCHES[@]}"; do
  if exists "$b"; then echo "bỏ qua (đã có): $b"; continue; fi
  run git branch "$b" "origin/$BASE"
  run git push -u origin "$b"
done

# 3) Branch protection cho main và develop (cần gh CLI đã đăng nhập: gh auth login)
#    Gói GitHub Free + repo private thường không hỗ trợ -> khi đó coi là quy ước của nhóm.
protect() {
  gh api -X PUT "repos/tvyyyy/NT106/branches/$1/protection" --input - >/dev/null <<'JSON'
{
  "required_status_checks": null,
  "enforce_admins": false,
  "required_pull_request_reviews": { "required_approving_review_count": 1 },
  "restrictions": null
}
JSON
}
if command -v gh >/dev/null 2>&1 && [ $DRY -eq 0 ]; then
  for br in main develop; do
    if protect "$br"; then echo "đã bật protection: $br"
    else echo "KHÔNG bật được protection cho $br (gói Free/private hoặc thiếu quyền). Bật tay: Settings > Branches"; fi
  done
else
  echo "Bỏ qua protection (không có gh hoặc đang --dry). Bật tay: GitHub > Settings > Branches > Add rule cho main và develop, tick 'Require a pull request before merging' (1 approval)."
fi

echo; echo "Nhánh trên origin:"
run git ls-remote --heads origin | awk '{print $2}' | sed 's#refs/heads/##'
