#!/usr/bin/env bash
# Kiểm tra CSDL bobashop chạy trong Docker: cấu trúc, ràng buộc, seed, test vi phạm ràng buộc
CONTAINER="${CONTAINER:-bobashop-pg}"
PSQL=(docker exec -i "$CONTAINER" psql -U postgres -d bobashop -v ON_ERROR_STOP=1)

PASS=0; FAIL=0
ok()  { echo "  [PASS] $1"; PASS=$((PASS+1)); }
bad() { echo "  [FAIL] $1"; FAIL=$((FAIL+1)); }

q() { "${PSQL[@]}" -tA; }

expect() {
  if [ "$3" = "$2" ]; then ok "$1 = $3"; else bad "$1: mong đợi $2, thực tế $3"; fi
}

expect_fail() {
  local out
  out=$("${PSQL[@]}" 2>&1)
  if echo "$out" | grep -q "violates"; then
    ok "$1 -> bị từ chối đúng"
  else
    bad "$1 -> KHÔNG bị chặn"
  fi
}

echo "== 1. Cấu trúc và ràng buộc =="
expect "Số bảng" 10 "$(q <<'SQL'
SELECT count(*) FROM information_schema.tables
WHERE table_schema='public' AND table_name <> '__EFMigrationsHistory';
SQL
)"
expect "Khóa chính (PK, gồm bảng history)" 11 "$(q <<'SQL'
SELECT count(*) FROM pg_constraint
WHERE connamespace='public'::regnamespace AND contype='p';
SQL
)"
expect "Khóa ngoại (FK)" 11 "$(q <<'SQL'
SELECT count(*) FROM pg_constraint
WHERE connamespace='public'::regnamespace AND contype='f';
SQL
)"
expect "CHECK" 11 "$(q <<'SQL'
SELECT count(*) FROM pg_constraint
WHERE connamespace='public'::regnamespace AND contype='c';
SQL
)"
expect "UNIQUE index" 9 "$(q <<'SQL'
SELECT count(*) FROM pg_indexes
WHERE schemaname='public' AND indexdef LIKE 'CREATE UNIQUE%' AND indexname NOT LIKE 'PK\_%';
SQL
)"

echo "== 2. Kiểu dữ liệu =="
expect "Roles.RoleName" "character varying(30)" "$(q <<'SQL'
SELECT data_type || '(' || character_maximum_length || ')' FROM information_schema.columns
WHERE table_name='Roles' AND column_name='RoleName';
SQL
)"
expect "Users.Email" "character varying(150)" "$(q <<'SQL'
SELECT data_type || '(' || character_maximum_length || ')' FROM information_schema.columns
WHERE table_name='Users' AND column_name='Email';
SQL
)"
expect "Products.Price" "numeric(18,2)" "$(q <<'SQL'
SELECT data_type || '(' || numeric_precision || ',' || numeric_scale || ')' FROM information_schema.columns
WHERE table_name='Products' AND column_name='Price';
SQL
)"
expect "Feedbacks có cột ProductID" 1 "$(q <<'SQL'
SELECT count(*) FROM information_schema.columns
WHERE table_name='Feedbacks' AND column_name='ProductID';
SQL
)"

echo "== 3. Dữ liệu seed =="
expect "Số dòng Roles" 2 "$(q <<'SQL'
SELECT count(*) FROM "Roles";
SQL
)"

echo "== 4. Test vi phạm ràng buộc (mỗi lệnh phải bị từ chối) =="
expect_fail "UNIQUE: trùng tên role" <<'SQL'
BEGIN;
INSERT INTO "Roles"("RoleName") VALUES ('Admin');
ROLLBACK;
SQL
expect_fail "CHECK: giá topping âm" <<'SQL'
BEGIN;
INSERT INTO "Toppings"("ToppingName","Price","IsAvailable") VALUES ('Test',-1,true);
ROLLBACK;
SQL
expect_fail "CHECK: voucher giảm giá = 0" <<'SQL'
BEGIN;
INSERT INTO "Vouchers"("Code","DiscountValue","MinOrderValue","MaxDiscountAmount","Quantity","IsActive","ExpiryDate","CreatedAt")
VALUES ('T1',0,0,0,1,true,NOW(),NOW());
ROLLBACK;
SQL
expect_fail "FK: danh mục không tồn tại" <<'SQL'
BEGIN;
INSERT INTO "Products"("CategoryID","ProductName","Price","IsAvailable") VALUES (999,'Ma',1000,true);
ROLLBACK;
SQL
expect_fail "FK: role không tồn tại" <<'SQL'
BEGIN;
INSERT INTO "Users"("RoleID","FullName","Phone","Email","PasswordHash","Status","CreatedAt")
VALUES (99,'A','0911111111','a@a.com','x','Active',NOW());
ROLLBACK;
SQL

echo
echo "KẾT QUẢ: $PASS đạt, $FAIL lỗi"
[ "$FAIL" -eq 0 ]
