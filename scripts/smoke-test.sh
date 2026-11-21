#!/usr/bin/env bash
# Smoke test for the ECommercePoc API. Requires only bash and curl.
#
# Self-contained mode (default): builds the API, starts it on $SMOKE_PORT with a throwaway
# SQLite database, runs the checks, and stops it. Doesn't touch your database or a running API.
#   ./scripts/smoke-test.sh
#
# Existing-API mode: run against an API you already started.
#   BASE_URL=http://localhost:5251 CUSTOMER_ID=<seeded-customer-id> ./scripts/smoke-test.sh
#
# Exit code: 0 if every check passes, 1 otherwise.

set -uo pipefail

SMOKE_PORT="${SMOKE_PORT:-5299}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_PID=""
WORK_DIR=""

cleanup() {
  if [[ -n "$API_PID" ]]; then
    kill "$API_PID" 2>/dev/null
    wait "$API_PID" 2>/dev/null
  fi
  if [[ -n "$WORK_DIR" ]]; then
    rm -rf "$WORK_DIR"
  fi
}
trap cleanup EXIT

if [[ -z "${BASE_URL:-}" ]]; then
  WORK_DIR="$(mktemp -d)"
  LOG_FILE="$WORK_DIR/api.log"
  BASE_URL="http://localhost:$SMOKE_PORT"

  echo "Building API..."
  # Build into a temp folder so a running API (which locks bin/) doesn't break the build.
  if ! dotnet build "$REPO_ROOT/src/ECommercePoc.Api" -c Debug -nologo -v quiet -o "$WORK_DIR/bin" > "$WORK_DIR/build.log" 2>&1; then
    cat "$WORK_DIR/build.log"
    echo "Build failed." >&2
    exit 1
  fi

  # Git Bash on Windows: environment variables aren't path-converted, so give .NET a Windows path.
  DB_PATH="$WORK_DIR/smoke.db"
  if command -v cygpath > /dev/null 2>&1; then
    DB_PATH="$(cygpath -m "$DB_PATH")"
  fi

  echo "Starting API on $BASE_URL with a temporary database..."
  # Run the built DLL from its output folder so appsettings.json is found.
  (
    cd "$WORK_DIR/bin" &&
    ASPNETCORE_ENVIRONMENT=Development \
    ConnectionStrings__Default="Data Source=$DB_PATH" \
    exec dotnet ECommercePoc.Api.dll --urls "$BASE_URL"
  ) > "$LOG_FILE" 2>&1 &
  API_PID=$!

  for _ in $(seq 1 60); do
    if grep -q "customer (ID: " "$LOG_FILE" 2>/dev/null && curl -s -o /dev/null "$BASE_URL/api/v1/products"; then
      break
    fi
    if ! kill -0 "$API_PID" 2>/dev/null; then
      cat "$LOG_FILE"
      echo "API exited during startup." >&2
      exit 1
    fi
    sleep 1
  done

  CUSTOMER_ID="$(grep -o 'customer (ID: [0-9a-f-]*' "$LOG_FILE" | head -1 | sed 's/.*ID: //')"
  if [[ -z "$CUSTOMER_ID" ]]; then
    cat "$LOG_FILE"
    echo "API did not start (or did not seed) within 60s." >&2
    exit 1
  fi
elif [[ -z "${CUSTOMER_ID:-}" ]]; then
  echo "CUSTOMER_ID is required when BASE_URL is set (see the 'Seeded ... customer (ID: ...)' startup log line)." >&2
  exit 1
fi

API="$BASE_URL/api/v1"
UNKNOWN_ID="00000000-0000-0000-0000-000000000001"
# SKUs are unique, so use a fresh one per run to keep the script re-runnable against the same database.
SMOKE_SKU="SMOKE-$(date +%s)-$RANDOM"
PASSED=0
FAILED=0

# request METHOD URL [BODY] [TOKEN] -> sets STATUS and BODY
request() {
  local method="$1" url="$2" data="${3:-}" token="${4:-}"
  local args=(-s -X "$method" -w $'\n%{http_code}')
  [[ -n "$token" ]] && args+=(-H "Authorization: Bearer $token")
  [[ -n "$data" ]] && args+=(-H "Content-Type: application/json" -d "$data")
  local response
  response="$(curl "${args[@]}" "$url")"
  STATUS="${response##*$'\n'}"
  BODY="${response%$'\n'*}"
}

# check NAME EXPECTED_STATUS [EXPECTED_SUBSTRING]
check() {
  local name="$1" expected_status="$2" expected_text="${3:-}"
  if [[ "$STATUS" == "$expected_status" && ( -z "$expected_text" || "$BODY" == *"$expected_text"* ) ]]; then
    echo "PASS  $name"
    PASSED=$((PASSED + 1))
  else
    echo "FAIL  $name (expected $expected_status${expected_text:+ containing '$expected_text'}, got $STATUS: ${BODY:0:200})"
    FAILED=$((FAILED + 1))
  fi
}

# First "field":"value" in BODY. Matches the first occurrence, so a nested object
# (e.g. an order item's "id") doesn't win over the top-level field.
json_field() { grep -o "\"$1\":\"[^\"]*\"" <<< "$BODY" | head -1 | cut -d'"' -f4; }

echo "Running smoke tests against $API"
echo

# Auth
request POST "$API/auth/login" '{"username":"admin","password":"wrong"}'
check "login with wrong password -> 400" 400 "Invalid username or password."

request POST "$API/auth/login" '{"username":"admin","password":"admin123"}'
check "login -> 200 with token" 200 '"token":"'
TOKEN="$(json_field token)"

# Products
request GET "$API/products"
check "list products -> 200" 200 '"sku":"'
PRODUCT_ID="$(sed -n 's/^\[{"id":"\([^"]*\)".*/\1/p' <<< "$BODY")"

request POST "$API/products" '{"name":"Smoke Test Product","description":"Created by smoke-test.sh","price":9.99,"currency":"USD","sku":"SMOKE-001"}'
check "create product without token -> 401" 401

request POST "$API/products" '{"name":"","price":-1,"sku":""}' "$TOKEN"
check "create invalid product -> 400" 400 "Validation Failed"

request POST "$API/products" '{"name":"Smoke Test Product","description":"Created by smoke-test.sh","price":9.99,"currency":"USD","sku":"'"$SMOKE_SKU"'"}' "$TOKEN"
check "create product -> 201" 201 "\"sku\":\"$SMOKE_SKU\""

# Orders
request POST "$API/orders" "{\"customerId\":\"$CUSTOMER_ID\",\"items\":[]}" "$TOKEN"
check "create empty order -> 400" 400 "Order must contain at least one item."

request POST "$API/orders" "{\"customerId\":\"$UNKNOWN_ID\",\"items\":[{\"productId\":\"$PRODUCT_ID\",\"quantity\":1}]}" "$TOKEN"
check "create order for unknown customer -> 404" 404 "Customer with id"

request POST "$API/orders" "{\"customerId\":\"$CUSTOMER_ID\",\"items\":[{\"productId\":\"$UNKNOWN_ID\",\"quantity\":1}]}" "$TOKEN"
check "create order with unknown product -> 404" 404 "Product with id"

request POST "$API/orders" "{\"customerId\":\"$CUSTOMER_ID\",\"items\":[{\"productId\":\"$PRODUCT_ID\",\"quantity\":2}]}" "$TOKEN"
check "create order -> 201 Pending" 201 '"status":"Pending"'
ORDER_ID="$(json_field id)"

request GET "$API/orders/$ORDER_ID"
check "get order without token -> 401" 401

request GET "$API/orders/$ORDER_ID" "" "$TOKEN"
check "get order -> 200" 200 "\"id\":\"$ORDER_ID\""

request GET "$API/orders/$UNKNOWN_ID" "" "$TOKEN"
check "get unknown order -> 404" 404 "Order with id"

request POST "$API/orders/$ORDER_ID/cancel" "" "$TOKEN"
check "cancel order -> 200 Cancelled" 200 '"status":"Cancelled"'

request POST "$API/orders/$ORDER_ID/cancel" "" "$TOKEN"
check "cancel again -> 400" 400 "Order is already cancelled."

echo
echo "$PASSED passed, $FAILED failed"
[[ "$FAILED" -eq 0 ]]
