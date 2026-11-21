# Runbook

How to run the API locally, call every endpoint with `curl`, check the expected responses, and fix common problems.

- [Start, stop, reset](#start-stop-reset)
- [Quick smoke test](#quick-smoke-test)
- [Manual walkthrough with curl](#manual-walkthrough-with-curl)
- [Error cases](#error-cases)
- [Other ways to call the API](#other-ways-to-call-the-api)
- [Troubleshooting](#troubleshooting)
- [Known issues](#known-issues)

All examples use Git Bash / Linux / macOS shell syntax. On PowerShell, use `curl.exe` (not the `curl` alias) and replace `\` line continuations with a backtick.

## Start, stop, reset

| Task | Command |
|------|---------|
| Start (HTTP) | `dotnet run --project src/ECommercePoc.Api` → `http://localhost:5251` |
| Start (HTTPS) | `dotnet run --project src/ECommercePoc.Api --launch-profile https` → `https://localhost:7082` |
| Stop | `Ctrl+C` in the terminal running the API |
| Reset data | Stop the API, delete `src/ECommercePoc.Api/ECommercePoc.db*`, start again |
| Run unit + integration tests | `dotnet test` |

Wait for `Now listening on: http://localhost:5251` in the console before sending requests.

On a fresh database, startup logs the seeded customer ID. Save it, because you need it to create orders:

```
info: ECommercePoc.Api[0]
      Seeded 5 products and 1 customer (ID: 6a24685a-d047-4b5f-ad62-eb4b287f4e1a)
```

This line appears **only on the first start**. Later starts log `Database already seeded — skipping.` If you lost the ID, [reset the data](#start-stop-reset).

## Quick smoke test

[`scripts/smoke-test.sh`](../scripts/smoke-test.sh) runs the full happy path plus the main error cases and prints `PASS`/`FAIL` for each check. It needs only `bash` and `curl`.

```bash
# Self-contained: builds the API, starts it on port 5299 with a throwaway database,
# runs the checks, then stops it. Doesn't touch your local database or a running API.
./scripts/smoke-test.sh

# Against an API you already started (you must supply the seeded customer ID)
BASE_URL=http://localhost:5251 CUSTOMER_ID=<customer-id> ./scripts/smoke-test.sh
```

Exit code `0` means every check passed.

## Manual walkthrough with curl

Set a base URL once:

```bash
BASE=http://localhost:5251/api/v1
```

### 1. Log in

```bash
curl -s -X POST $BASE/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

`200 OK`:

```json
{"token":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...","expiresAt":"2026-11-21T04:33:57.8627715Z"}
```

Save the token (valid for 1 hour):

```bash
TOKEN=$(curl -s -X POST $BASE/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | sed 's/.*"token":"\([^"]*\)".*/\1/')
```

### 2. List products (no auth)

```bash
curl -s $BASE/products
```

`200 OK`, with 5 seeded products. IDs are random on each fresh database:

```json
[
  {"id":"276a3241-9826-4c52-92b7-ed3eb2691428","name":"27\" 4K Monitor","description":"27-inch 4K IPS monitor with HDR","price":449.99,"currency":"USD","sku":"MON-4K-004"},
  {"id":"6ef27a3e-7770-4890-9761-b7157a4b5a77","name":"Wireless Mouse","description":"Ergonomic wireless mouse with USB-C charging","price":29.99,"currency":"USD","sku":"WM-001"},
  ...
]
```

Save one product ID:

```bash
PRODUCT_ID=$(curl -s $BASE/products | sed 's/^\[{"id":"\([^"]*\)".*/\1/')
```

### 3. Create a product (auth)

```bash
curl -s -X POST $BASE/products \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop Stand","description":"Adjustable aluminium laptop stand","price":39.99,"currency":"USD","sku":"LS-006"}'
```

`201 Created`:

```json
{"id":"7e81c48e-d0fe-4616-acda-1c5bb2131360","name":"Laptop Stand","description":"Adjustable aluminium laptop stand","price":39.99,"currency":"USD","sku":"LS-006"}
```

### 4. Create an order (auth)

```bash
CUSTOMER_ID=<customer-id-from-startup-log>

curl -s -i -X POST $BASE/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"customerId\":\"$CUSTOMER_ID\",\"items\":[{\"productId\":\"$PRODUCT_ID\",\"quantity\":2}]}"
```

`201 Created`, with a `Location` header pointing at the new order:

```
HTTP/1.1 201 Created
Location: http://localhost:5251/api/v1/orders/cdc35b8f-09ef-432d-b70d-fea0ce753003
```

```json
{
  "id": "cdc35b8f-09ef-432d-b70d-fea0ce753003",
  "customerId": "6a24685a-d047-4b5f-ad62-eb4b287f4e1a",
  "status": "Pending",
  "createdDate": "2026-11-21T03:29:43.0615812Z",
  "total": 899.98,
  "currency": "USD",
  "items": [
    {"id":"aae86597-99e4-4a1a-aea9-643821ea07ad","productId":"276a3241-9826-4c52-92b7-ed3eb2691428","productName":"27\" 4K Monitor","quantity":2,"unitPrice":449.99,"currency":"USD"}
  ]
}
```

The total is computed by the `Order` aggregate (2 × 449.99). Adding the same product twice in one order merges the lines into a single item.

Save the order ID:

```bash
ORDER_ID=<id-from-response>
```

### 5. Get the order (auth)

```bash
curl -s $BASE/orders/$ORDER_ID -H "Authorization: Bearer $TOKEN"
```

`200 OK`, with the same body as above.

> **Known quirk:** `createdDate` comes back with a `Z` suffix on create but without it on later reads. SQLite doesn't store the UTC marker. The value is UTC in both cases.

### 6. Cancel the order (auth)

```bash
curl -s -X POST $BASE/orders/$ORDER_ID/cancel -H "Authorization: Bearer $TOKEN"
```

`200 OK`, with the order now in `"status":"Cancelled"`.

## Error cases

Errors are returned as RFC 7807 problem details (`application/problem+json`), except `401`, which has an empty body and a `WWW-Authenticate: Bearer` header.

| Scenario | Request | Status | Response `title` / `detail` |
|----------|---------|--------|-----------------------------|
| Wrong password | `POST /auth/login` with `{"username":"admin","password":"wrong"}` | `400` | Business Rule Violation: `Invalid username or password.` |
| No token | `POST /orders` without `Authorization` | `401` | *(empty body)* |
| Invalid / expired token | `GET /orders/{id}` with `Authorization: Bearer abc` | `401` | *(empty body)* |
| Empty order | `POST /orders` with `"items":[]` | `400` | Validation Failed: `Items: Order must contain at least one item.` |
| Zero quantity | `POST /orders` with `"quantity":0` | `400` | Validation Failed: `Items[0].Quantity: Quantity must be greater than zero.` |
| Unknown customer | `POST /orders` with a random `customerId` | `404` | Resource Not Found: `Customer with id '...' was not found.` |
| Unknown product | `POST /orders` with a random `productId` | `404` | Resource Not Found: `Product with id '...' was not found.` |
| Unknown order | `GET /orders/{random-guid}` | `404` | Resource Not Found: `Order with id '...' was not found.` |
| Cancel twice | `POST /orders/{id}/cancel` on a cancelled order | `400` | Business Rule Violation: `Order is already cancelled.` |
| Invalid product | `POST /products` with `{"name":"","price":-1,"sku":""}` | `400` | Validation Failed: `Name: Product name is required.`, `Price: Price must be greater than zero.`, `Sku: SKU is required.` |
| Duplicate SKU | `POST /products` with a SKU that already exists | `409` | Conflict: `A product with SKU '...' already exists.` |
| Missing version in URL | `GET /api/products` | `404` | *(empty body)*. Always include `/v1/`. |

Example (cancel twice):

```bash
curl -s -X POST $BASE/orders/$ORDER_ID/cancel -H "Authorization: Bearer $TOKEN"
```

```json
{"title":"Business Rule Violation","status":400,"detail":"Order is already cancelled.","instance":"/api/v1/orders/cdc35b8f-09ef-432d-b70d-fea0ce753003/cancel"}
```

Seeing the HTTP status: add `-w " [%{http_code}]\n"` to any curl command, or use `-i` to print the status line and headers.

## Other ways to call the API

- **Swagger UI:** `http://localhost:5251/swagger`. Call `POST /api/v1/auth/login`, copy the token, click **Authorize**, and paste it (without `Bearer `). Endpoints with a lock then send it automatically.
- **`.http` file:** [`src/ECommercePoc.Api/ECommercePoc.Api.http`](../src/ECommercePoc.Api/ECommercePoc.Api.http) runs in Visual Studio 2022 (17.12+), JetBrains Rider, or VS Code with the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension. Set `@customerId` at the top, run **login** first, and the token, product ID, and order ID are passed between requests automatically.

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| `You must install or update .NET to run this application` / `Framework 'Microsoft.NETCore.App', version '8.0.0' was not found` | Only a newer runtime is installed. The projects target `net8.0`. | Install the .NET 8 and ASP.NET Core 8 runtimes, e.g. `winget install Microsoft.DotNet.Runtime.8 Microsoft.DotNet.AspNetCore.8`. They install side by side with newer versions. |
| `Failed to bind to address http://127.0.0.1:5251: address already in use` | Another instance (or app) is using the port | Stop the other instance, or run with `--urls http://localhost:5252` |
| Every protected call returns `401` | Missing `Bearer ` prefix, token expired (1 hour), or the API restarted with a different `Jwt:Secret` | Log in again and send `Authorization: Bearer <token>` |
| `401` from Swagger UI even after Authorize | Pasted `Bearer <token>` instead of just the token | Paste only the token. Swagger adds the prefix. |
| Can't find the customer ID | It's only logged when the database is first seeded | Stop the API, delete `src/ECommercePoc.Api/ECommercePoc.db*`, start again |
| `404` with an empty body | URL is missing `/v1/` or has a typo | Use `/api/v1/...` |
| Swagger UI returns `404` | Not running in `Development` | Start with `dotnet run` (launch profiles set `Development`), or set `ASPNETCORE_ENVIRONMENT=Development` |
| HTTPS certificate warning on `https://localhost:7082` | Dev certificate not trusted | `dotnet dev-certs https --trust`, or use the HTTP profile |
| Startup warning about a MediatR license | You upgraded MediatR to 13+ | Pin it back to 12.x (see README). 13+ is commercially licensed. |

## Known issues

| Issue | Behavior | Workaround |
|-------|----------|------------|
| `createdDate` loses its `Z` suffix | Create returns `...Z`, later reads don't, because SQLite doesn't store the UTC marker | Treat the value as UTC |
| No way to look up customers | There's no customer endpoint, and the seeded ID is only logged on first start | Reset the database to see it again |
