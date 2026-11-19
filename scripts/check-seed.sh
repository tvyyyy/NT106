#!/usr/bin/env bash
# Kiểm tra dữ liệu seed của hệ khuyến nghị (user @seed.local) trong CSDL bobashop chạy bằng Docker.
# Chạy sau: dotnet run --project src/Presentation/BobaShop.WebAPI --launch-profile http -- --seed
# Đổi tên container nếu khác: CONTAINER=bobashop-db bash scripts/check-seed.sh
CONTAINER="${CONTAINER:-bobashop-pg}"
PSQL=(docker exec -i "$CONTAINER" psql -U postgres -d bobashop -v ON_ERROR_STOP=1)

PASS=0; FAIL=0
ok()  { echo "  [PASS] $1"; PASS=$((PASS+1)); }
bad() { echo "  [FAIL] $1"; FAIL=$((FAIL+1)); }

q() { "${PSQL[@]}" -tA; }

expect() {
  if [ "$3" = "$2" ]; then ok "$1 = $3"; else bad "$1: mong đợi $2, thực tế $3"; fi
}

# expect_between <mô tả> <min> <max> <giá trị>
expect_between() {
  if [ -n "$4" ] && [ "$4" -ge "$2" ] && [ "$4" -le "$3" ]; then
    ok "$1 = $4 (trong khoảng $2-$3)"
  else
    bad "$1: mong đợi trong khoảng $2-$3, thực tế ${4:-rỗng}"
  fi
}

SEED_USERS="SELECT \"UserID\" FROM \"Users\" WHERE \"Email\" LIKE '%@seed.local'"
SEED_ORDERS="SELECT \"OrderID\" FROM \"Orders\" WHERE \"UserID\" IN ($SEED_USERS)"

echo "== 1. Số dòng =="
expect "User seed" 50 "$(q <<SQL
SELECT count(*) FROM ($SEED_USERS) u;
SQL
)"
expect "User seed có vai trò Customer" 50 "$(q <<SQL
SELECT count(*) FROM "Users" u JOIN "Roles" r ON r."RoleID" = u."RoleID"
WHERE u."Email" LIKE '%@seed.local' AND r."RoleName" = 'Customer';
SQL
)"
expect_between "Đơn seed" 400 800 "$(q <<SQL
SELECT count(*) FROM ($SEED_ORDERS) o;
SQL
)"
expect_between "Dòng chi tiết seed" 500 1300 "$(q <<SQL
SELECT count(*) FROM "OrderDetails" WHERE "OrderID" IN ($SEED_ORDERS);
SQL
)"
expect_between "Feedback seed" 80 400 "$(q <<SQL
SELECT count(*) FROM "Feedbacks" WHERE "UserID" IN ($SEED_USERS);
SQL
)"
expect "Danh mục trong catalog" 6 "$(q <<'SQL'
SELECT count(*) FROM "Categories" WHERE "CategoryName" IN
('Trà sữa','Trà trái cây','Cà phê','Đá xay','Trà nguyên bản','Sữa chua & đặc biệt');
SQL
)"
expect "Món hết hàng (Trà sữa socola, Trà gạo lứt)" 2 "$(q <<'SQL'
SELECT count(*) FROM "Products"
WHERE "ProductName" IN ('Trà sữa socola','Trà gạo lứt') AND "IsAvailable" = false;
SQL
)"

echo "== 2. Tiền và ràng buộc =="
expect "Đơn có FinalAmount <> TotalAmount - DiscountAmount" 0 "$(q <<SQL
SELECT count(*) FROM "Orders" WHERE "OrderID" IN ($SEED_ORDERS)
AND "FinalAmount" <> "TotalAmount" - "DiscountAmount";
SQL
)"
expect "Đơn có TotalAmount khác tổng (UnitPrice + topping) x Quantity" 0 "$(q <<SQL
WITH lines AS (
  SELECT d."OrderID",
         (d."UnitPrice" + COALESCE((SELECT sum(t."Price") FROM "OrderDetailToppings" t
                                    WHERE t."OrderDetailID" = d."OrderDetailID"), 0)) * d."Quantity" AS amount
  FROM "OrderDetails" d WHERE d."OrderID" IN ($SEED_ORDERS)
)
SELECT count(*) FROM "Orders" o
JOIN (SELECT "OrderID", sum(amount) AS total FROM lines GROUP BY "OrderID") l ON l."OrderID" = o."OrderID"
WHERE o."TotalAmount" <> l.total;
SQL
)"
expect "Đơn seed không có dòng chi tiết" 0 "$(q <<SQL
SELECT count(*) FROM "Orders" o WHERE o."OrderID" IN ($SEED_ORDERS)
AND NOT EXISTS (SELECT 1 FROM "OrderDetails" d WHERE d."OrderID" = o."OrderID");
SQL
)"
expect "Giá topping chốt khác giá topping hiện tại" 0 "$(q <<SQL
SELECT count(*) FROM "OrderDetailToppings" x JOIN "Toppings" t ON t."ToppingID" = x."ToppingID"
JOIN "OrderDetails" d ON d."OrderDetailID" = x."OrderDetailID"
WHERE d."OrderID" IN ($SEED_ORDERS) AND x."Price" <> t."Price";
SQL
)"

echo "== 3. Feedback =="
expect "Feedback trùng (OrderID, ProductID)" 0 "$(q <<'SQL'
SELECT count(*) FROM (SELECT 1 FROM "Feedbacks" GROUP BY "OrderID","ProductID" HAVING count(*) > 1) x;
SQL
)"
expect "Feedback cho món không nằm trong đơn của chính user" 0 "$(q <<SQL
SELECT count(*) FROM "Feedbacks" f JOIN "Orders" o ON o."OrderID" = f."OrderID"
WHERE f."UserID" IN ($SEED_USERS)
AND (o."UserID" <> f."UserID"
     OR NOT EXISTS (SELECT 1 FROM "OrderDetails" d WHERE d."OrderID" = f."OrderID" AND d."ProductID" = f."ProductID"));
SQL
)"
expect "Feedback trên đơn chưa Completed" 0 "$(q <<SQL
SELECT count(*) FROM "Feedbacks" f JOIN "Orders" o ON o."OrderID" = f."OrderID"
WHERE f."UserID" IN ($SEED_USERS) AND o."Status" <> 'Completed';
SQL
)"
expect "Trà ổi hồng (món mới): số feedback 5 sao" "2|2" "$(q <<'SQL'
SELECT count(*) || '|' || count(*) FILTER (WHERE f."Rating" = 5)
FROM "Feedbacks" f JOIN "Products" p ON p."ProductID" = f."ProductID" WHERE p."ProductName" = 'Trà ổi hồng';
SQL
)"
expect "Hồng trà sữa: trung bình sao trong khoảng 4.0-4.6" t "$(q <<'SQL'
SELECT avg(f."Rating") BETWEEN 4.0 AND 4.6
FROM "Feedbacks" f JOIN "Products" p ON p."ProductID" = f."ProductID" WHERE p."ProductName" = 'Hồng trà sữa';
SQL
)"
expect "Trà sữa Thái xanh: trung bình sao trong khoảng 2.6-3.8" t "$(q <<'SQL'
SELECT avg(f."Rating") BETWEEN 2.6 AND 3.8
FROM "Feedbacks" f JOIN "Products" p ON p."ProductID" = f."ProductID" WHERE p."ProductName" = 'Trà sữa Thái xanh';
SQL
)"

echo "== 4. Phân phối dùng cho khuyến nghị =="
expect "Tỷ lệ đơn Completed trong khoảng 80-95%" t "$(q <<SQL
SELECT avg(CASE WHEN "Status" = 'Completed' THEN 1.0 ELSE 0 END) BETWEEN 0.80 AND 0.95
FROM "Orders" WHERE "OrderID" IN ($SEED_ORDERS);
SQL
)"
# Phân phối đều thì 5 món chiếm khoảng 5/33 = 15% số ly. Với trọng số phổ biến trong catalog,
# mô phỏng cho ra khoảng 22-29%, nên lấy ngưỡng 20% (cao hơn mức đều khoảng 1,3 lần trở lên).
expect "Top 5 món chiếm từ 20% số ly trở lên (phân phối lệch so với mức đều 15%)" t "$(q <<SQL
WITH s AS (
  SELECT d."ProductID", sum(d."Quantity") AS qty FROM "OrderDetails" d
  JOIN "Orders" o ON o."OrderID" = d."OrderID"
  WHERE o."OrderID" IN ($SEED_ORDERS) AND o."Status" = 'Completed' GROUP BY d."ProductID"
)
SELECT (SELECT sum(qty) FROM (SELECT qty FROM s ORDER BY qty DESC LIMIT 5) t) >= 0.20 * (SELECT sum(qty) FROM s);
SQL
)"
expect "User seed chưa có đơn" 7 "$(q <<SQL
SELECT count(*) FROM "Users" u WHERE u."Email" LIKE '%@seed.local'
AND NOT EXISTS (SELECT 1 FROM "Orders" o WHERE o."UserID" = u."UserID");
SQL
)"
expect "User seed có 1-2 đơn (phải lùi về Cấp 1)" 8 "$(q <<SQL
SELECT count(*) FROM (SELECT o."UserID" FROM "Orders" o WHERE o."UserID" IN ($SEED_USERS)
GROUP BY o."UserID" HAVING count(*) BETWEEN 1 AND 2) x;
SQL
)"
expect "Món hết hàng có đơn trong 14 ngày gần nhất" 0 "$(q <<SQL
SELECT count(*) FROM "OrderDetails" d JOIN "Orders" o ON o."OrderID" = d."OrderID"
JOIN "Products" p ON p."ProductID" = d."ProductID"
WHERE o."OrderID" IN ($SEED_ORDERS) AND p."IsAvailable" = false AND o."CreatedAt" >= NOW() - INTERVAL '14 days';
SQL
)"
expect "Đơn seed có thời điểm trong tương lai" 0 "$(q <<SQL
SELECT count(*) FROM "Orders" WHERE "OrderID" IN ($SEED_ORDERS) AND "CreatedAt" > NOW();
SQL
)"

# Products.CreatedAt chỉ có sau migration AddProductCreatedAt (việc STT 1).
HAS_CREATED_AT="$(q <<'SQL'
SELECT count(*) FROM information_schema.columns WHERE table_name='Products' AND column_name='CreatedAt';
SQL
)"
if [ "$HAS_CREATED_AT" = "1" ]; then
  expect "Món mới trong 14 ngày (Trà ổi hồng, Cold brew cam, Dâu đá xay, Yakult đào)" 4 "$(q <<'SQL'
SELECT count(*) FROM "Products"
WHERE "ProductName" IN ('Trà ổi hồng','Cold brew cam','Dâu đá xay','Yakult đào')
AND "CreatedAt" >= NOW() - INTERVAL '14 days';
SQL
)"
else
  echo "  [SKIP] Chưa có cột Products.CreatedAt, bỏ qua kiểm tra món mới"
fi

echo
echo "KẾT QUẢ: $PASS đạt, $FAIL lỗi"
[ "$FAIL" -eq 0 ]
