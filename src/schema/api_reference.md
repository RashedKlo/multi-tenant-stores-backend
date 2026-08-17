# API Reference — Customer-Facing Backend

Conventions: all list endpoints support `?page=&pageSize=`. Auth column: **Public** = no token needed, **Guest** = needs a guest session token, **Auth** = needs a valid customer JWT, **Either** = works with either.

## 1. Auth

| Method & Path | Auth | Purpose |
|---|---|---|
| `POST /api/auth/guest-session` | Public | Create a guest session, returns opaque token (store in cookie/localStorage) |
| `POST /api/auth/register` | Public | Create account: firstName, lastName, email, password. Sends verification code, returns short-lived registration token (not a full session yet) |
| `POST /api/auth/verify-email` | Public | Body: email, code. Marks `is_email_verified = true`, returns JWT + refresh token |
| `POST /api/auth/resend-verification` | Public | Re-sends a new code (rate-limit this) |
| `POST /api/auth/login` | Public | email + password → JWT + refresh token. On success: run guest→auth cart handoff (see §4) |
| `POST /api/auth/google` | Public | Body: Google ID token → creates or logs in customer → JWT + refresh token. Same cart handoff as login |
| `POST /api/auth/refresh` | Public | Body: refresh token → new JWT (+ rotate refresh token) |
| `POST /api/auth/logout` | Auth | Revokes the current refresh token (`revoked_at = now()`) |
| `POST /api/auth/forgot-password` | Public | Body: email → sends reset code/link |
| `POST /api/auth/reset-password` | Public | Body: email, code, newPassword |

## 2. Addresses

| Method & Path | Auth | Purpose |
|---|---|---|
| `GET /api/addresses` | Auth | List all addresses for current customer |
| `POST /api/addresses` | Auth | Create: label, latitude, longitude, addressText, isDefault |
| `GET /api/addresses/{id}` | Auth | One address (used when opening the edit screen) |
| `PUT /api/addresses/{id}` | Auth | Update fields |
| `DELETE /api/addresses/{id}` | Auth | Soft delete (`deleted_at`) — block if referenced by a pending order, otherwise allow |
| `POST /api/addresses/{id}/set-default` | Auth | Unsets previous default, sets this one |

## 3. Home / Discovery

| Method & Path | Auth | Purpose |
|---|---|---|
| `GET /api/home/banners` | Public | Active home banners, ordered |
| `GET /api/modules` | Public | Active modules (Restaurants, Markets, Pharmacies…), ordered |
| `GET /api/modules/{id}` | Public | Module detail + its `module_banners` + its `categories` |
| `GET /api/modules/{id}/stores?categoryId=&search=` | Either | Stores in this module, optionally filtered by category. If **Auth**, includes `isFavorite` per store |

## 4. Store & Catalog Browsing

| Method & Path | Auth | Purpose |
|---|---|---|
| `GET /api/stores/{id}` | Either | Store details (name, banner, rating, address). Includes `isFavorite` if Auth |
| `GET /api/stores/{id}/banners` | Public | `store_banners` |
| `GET /api/stores/{id}/sections?page=&pageSize=` | Public | Paginated sections for "load more" |
| `GET /api/sections/{id}/products?status=&minPrice=&maxPrice=&page=` | Either | Products in a section. `status` = active/out-of-stock. Includes `isFavorite` per product if Auth |
| `GET /api/products/{id}` | Either | Full product detail — images, option groups + values, `isFavorite` if Auth. (This is the "precomputed JSON" read we designed earlier) |

## 5. Favorites

| Method & Path | Auth | Purpose |
|---|---|---|
| `POST /api/favorites/products/{productId}` | Auth | Add to favorites |
| `DELETE /api/favorites/products/{productId}` | Auth | Remove |
| `GET /api/favorites/products` | Auth | List favorite products (for "My Favorites" page) |
| `POST /api/favorites/stores/{storeId}` | Auth | Add store to favorites |
| `DELETE /api/favorites/stores/{storeId}` | Auth | Remove |
| `GET /api/favorites/stores` | Auth | List favorite stores |

## 6. Cart

Cart is resolved server-side from the JWT (Auth) or guest token (Guest) + `storeId` — never pass a raw `cartId` from the client.

| Method & Path | Auth | Purpose |
|---|---|---|
| `GET /api/cart?storeId=` | Either | Current cart for this store: items, quantities, selected options, live prices, live stock |
| `POST /api/cart/items` | Either | Add item: storeId, productId, quantity, optionIds[], notes. **Server-side**: if an identical product+option combination already exists in the cart, increment quantity instead of creating a new line (see §7) |
| `PUT /api/cart/items/{itemId}` | Either | Update quantity/notes |
| `DELETE /api/cart/items/{itemId}` | Either | Remove a line |
| `DELETE /api/cart?storeId=` | Either | Clear the whole cart for that store |

## 7. Checkout & Orders

| Method & Path | Auth | Purpose |
|---|---|---|
| `POST /api/checkout` | Auth | Body: storeId, addressId. Runs the full checkout transaction (see below), creates the order + a Stripe PaymentIntent, returns `clientSecret` for the frontend Stripe SDK |
| `POST /api/webhooks/stripe` | Public (Stripe-signed) | Stripe calls this on `payment_intent.succeeded` / `.payment_failed` → updates `payments.status`, and on success moves order to `Confirmed` + writes `order_status_history` |
| `GET /api/orders?status=&page=` | Auth | "My Orders" list, filterable by status |
| `GET /api/orders/{id}` | Auth | Order detail: items, options, status history, payment status |

**What `POST /api/checkout` actually does (application logic, not new tables):**
1. Load cart + lock referenced products (`SELECT ... FOR UPDATE`)
2. Re-check each product/option is still active and in stock
3. Re-read current prices/discounts — never trust cart-displayed totals
4. Validate each option group's `min_selection`/`max_selection` is satisfied
5. Create `orders` + `order_items` + `order_item_options` (snapshotting names/prices)
6. Decrement `stock_quantity` for tracked products
7. Create Stripe PaymentIntent, insert `payments` row
8. Clear the cart
9. Commit the transaction — all of 1–8 succeed together or none do

## 8. Settings

| Method & Path | Auth | Purpose |
|---|---|---|
| `POST /api/auth/logout` | Auth | (listed above — same endpoint covers "Logout" in Settings) |
| `POST /api/auth/forgot-password` | Public | (listed above — same endpoint covers "Forgot Password" in Settings) |
| `PUT /api/customers/me` | Auth | Update firstName/lastName |
| `PUT /api/customers/me/password` | Auth | Change password while logged in (current + new password) |

---

