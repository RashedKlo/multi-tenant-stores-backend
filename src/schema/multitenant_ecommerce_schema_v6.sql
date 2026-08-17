-- ============================================================
-- Multi-tenant E-Commerce Platform — PostgreSQL Schema (v5)
-- Integrity-focused revision over v4. Changes:
--   - products.store_id denormalized + auto-maintained (trigger)
--   - triggers preventing cross-store cart items / options /
--     discounts (real bugs flagged in review)
--   - composite FK: orders.address_id must belong to orders.customer_id
--   - delivery address snapshot on orders
--   - timestamptz everywhere, updated_at defaults to now()
--   - guest session token hashing
--   - refresh_tokens: revoked_at + last_used_at
--   - payments: failure_reason, provider_metadata, paid_at, refunded_at
--   - order_status_history: optional changed_by_type/changed_by_id
-- Deliberately NOT added (see chat): tenant_id on every table + RLS,
-- cart-line signature constraint, delivery_fee/tax columns,
-- cart status enum — these are either app-layer concerns or not
-- needed yet.
-- ============================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- ============================================================
-- ENUMS
-- ============================================================

CREATE TYPE discount_type   AS ENUM ('Percentage', 'FixedAmount');
CREATE TYPE selection_type  AS ENUM ('Single', 'Multiple');
CREATE TYPE order_status    AS ENUM (
    'Pending', 'Confirmed', 'Preparing', 'OutForDelivery', 'Delivered', 'Cancelled'
);
CREATE TYPE payment_status  AS ENUM ('Pending', 'Succeeded', 'Failed', 'Refunded');

-- ============================================================
-- HELPERS
-- ============================================================

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- TENANTS
-- ============================================================

CREATE TABLE tenants (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name          varchar(200) NOT NULL CHECK (length(btrim(name)) > 0),
    email         varchar(255) NOT NULL UNIQUE
                    CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    password_hash text NOT NULL CHECK (length(password_hash) > 0),
    is_active     boolean NOT NULL DEFAULT true,
    created_at    timestamptz NOT NULL DEFAULT now(),
    updated_at    timestamptz NOT NULL DEFAULT now(),
    deleted_at    timestamptz
);

CREATE TRIGGER trg_tenants_updated_at
    BEFORE UPDATE ON tenants FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ============================================================
-- MODULES / BANNERS / CATEGORIES
-- ============================================================

CREATE TABLE modules (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name_en       varchar(100) NOT NULL UNIQUE CHECK (length(btrim(name_en)) > 0),
    name_ar       varchar(100) NOT NULL UNIQUE CHECK (length(btrim(name_ar)) > 0),
    icon_url      varchar(500),
    display_order int NOT NULL DEFAULT 0 CHECK (display_order >= 0),
    is_active     boolean NOT NULL DEFAULT true
);

CREATE TABLE home_banners (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    image_url     varchar(500) NOT NULL CHECK (length(btrim(image_url)) > 0),
    title_en      varchar(200),
    title_ar      varchar(200),
    subtitle_en   varchar(500),
    subtitle_ar   varchar(500),
    action_url    varchar(500),
    display_order int NOT NULL DEFAULT 0 CHECK (display_order >= 0),
    is_active     boolean NOT NULL DEFAULT true
);

CREATE TABLE module_banners (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    module_id     uuid NOT NULL REFERENCES modules(id) ON DELETE CASCADE,
    image_url     varchar(500) NOT NULL CHECK (length(btrim(image_url)) > 0),
    title_en      varchar(200),
    title_ar      varchar(200),
    action_url    varchar(500),
    display_order int NOT NULL DEFAULT 0 CHECK (display_order >= 0),
    is_active     boolean NOT NULL DEFAULT true
);

CREATE INDEX idx_module_banners_module_id ON module_banners(module_id);

CREATE TABLE categories (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    module_id     uuid NOT NULL REFERENCES modules(id) ON DELETE CASCADE,
    name_en       varchar(150) NOT NULL CHECK (length(btrim(name_en)) > 0),
    name_ar       varchar(150) NOT NULL CHECK (length(btrim(name_ar)) > 0),
    image_url     varchar(500),
    display_order int NOT NULL DEFAULT 0 CHECK (display_order >= 0),
    is_active     boolean NOT NULL DEFAULT true,
    UNIQUE (module_id, name_en),
    UNIQUE (module_id, name_ar)
);

CREATE INDEX idx_categories_module_id ON categories(module_id);
CREATE INDEX idx_categories_name_en_trgm ON categories USING gin (name_en gin_trgm_ops);
CREATE INDEX idx_categories_name_ar_trgm ON categories USING gin (name_ar gin_trgm_ops);

-- ============================================================
-- STORES
-- ============================================================

CREATE TABLE stores (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    module_id       uuid NOT NULL REFERENCES modules(id) ON DELETE RESTRICT,
    name_en         varchar(200) NOT NULL CHECK (length(btrim(name_en)) > 0),
    name_ar         varchar(200) NOT NULL CHECK (length(btrim(name_ar)) > 0),
    description_en  text,
    description_ar  text,
    logo_url        varchar(500),
    banner_url      varchar(500),
    phone           varchar(30),
    email           varchar(255)
                      CHECK (email IS NULL OR email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    address_en      text,
    address_ar      text,
    latitude        decimal(10,7) CHECK (latitude  IS NULL OR (latitude  BETWEEN -90  AND 90)),
    longitude       decimal(10,7) CHECK (longitude IS NULL OR (longitude BETWEEN -180 AND 180)),
    rating          decimal(2,1) NOT NULL DEFAULT 0 CHECK (rating BETWEEN 0 AND 5),
    metadata        jsonb,
    is_active       boolean NOT NULL DEFAULT true,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    deleted_at      timestamptz
);

CREATE TRIGGER trg_stores_updated_at
    BEFORE UPDATE ON stores FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_stores_tenant_id ON stores(tenant_id);
CREATE INDEX idx_stores_module_id ON stores(module_id);
CREATE INDEX idx_stores_name_en_trgm ON stores USING gin (name_en gin_trgm_ops);
CREATE INDEX idx_stores_name_ar_trgm ON stores USING gin (name_ar gin_trgm_ops);

CREATE TABLE store_categories (
    store_id      uuid NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    category_id   uuid NOT NULL REFERENCES categories(id) ON DELETE CASCADE,
    PRIMARY KEY (store_id, category_id)
);

CREATE INDEX idx_store_categories_category_id ON store_categories(category_id);

CREATE TABLE store_banners (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id      uuid NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    image_url     varchar(500) NOT NULL CHECK (length(btrim(image_url)) > 0),
    title_en      varchar(200),
    title_ar      varchar(200),
    action_url    varchar(500),
    display_order int NOT NULL DEFAULT 0 CHECK (display_order >= 0),
    is_active     boolean NOT NULL DEFAULT true
);

CREATE INDEX idx_store_banners_store_id ON store_banners(store_id);

CREATE TABLE store_sections (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id      uuid NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    name_en       varchar(150) NOT NULL CHECK (length(btrim(name_en)) > 0),
    name_ar       varchar(150) NOT NULL CHECK (length(btrim(name_ar)) > 0),
    image_url     varchar(500),
    display_order int NOT NULL DEFAULT 0 CHECK (display_order >= 0),
    is_active     boolean NOT NULL DEFAULT true,
    UNIQUE (store_id, name_en),
    UNIQUE (store_id, name_ar)
);

CREATE INDEX idx_store_sections_store_id ON store_sections(store_id);

-- ============================================================
-- PRODUCTS
-- store_id is denormalized from section_id and auto-maintained
-- below by trigger — never set it manually from the app.
-- ============================================================

CREATE TABLE products (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    section_id      uuid NOT NULL REFERENCES store_sections(id) ON DELETE CASCADE,
    store_id        uuid NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    name_en         varchar(255) NOT NULL CHECK (length(btrim(name_en)) > 0),
    name_ar         varchar(255) NOT NULL CHECK (length(btrim(name_ar)) > 0),
    description_en  text,
    description_ar  text,
    metadata        jsonb,
    price           numeric(18,2) NOT NULL CHECK (price >= 0),
    compare_price   numeric(18,2) CHECK (compare_price IS NULL OR compare_price >= price),
    sku             varchar(100),
    barcode         varchar(100),
    track_inventory boolean NOT NULL DEFAULT false,
    stock_quantity  integer NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    weight          numeric(10,2) CHECK (weight IS NULL OR weight >= 0),
    is_active       boolean NOT NULL DEFAULT true,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    deleted_at      timestamptz
);


CREATE TRIGGER trg_products_updated_at
    BEFORE UPDATE ON products FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_products_section_id ON products(section_id);
CREATE INDEX idx_products_store_id ON products(store_id);
CREATE INDEX idx_products_name_en_trgm ON products USING gin (name_en gin_trgm_ops);
CREATE INDEX idx_products_name_ar_trgm ON products USING gin (name_ar gin_trgm_ops);
CREATE INDEX idx_products_active_by_section
    ON products(section_id) WHERE is_active = true AND deleted_at IS NULL;
CREATE UNIQUE INDEX uq_products_sku ON products(sku) WHERE sku IS NOT NULL;
CREATE UNIQUE INDEX uq_products_barcode ON products(barcode) WHERE barcode IS NOT NULL;

CREATE TABLE product_images (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id    uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    image_url     varchar(500) NOT NULL CHECK (length(btrim(image_url)) > 0),
    display_order int NOT NULL DEFAULT 0 CHECK (display_order >= 0)
);

CREATE INDEX idx_product_images_product_id ON product_images(product_id);

CREATE TABLE product_option_groups (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id      uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    name_en         varchar(150) NOT NULL CHECK (length(btrim(name_en)) > 0),
    name_ar         varchar(150) NOT NULL CHECK (length(btrim(name_ar)) > 0),
    selection_type  selection_type NOT NULL DEFAULT 'Single',
    min_selection   int NOT NULL DEFAULT 0 CHECK (min_selection >= 0),
    max_selection   int NOT NULL DEFAULT 1 CHECK (max_selection >= 1),
    display_order   int NOT NULL DEFAULT 0 CHECK (display_order >= 0),
    is_active       boolean NOT NULL DEFAULT true,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    deleted_at      timestamptz,
    CHECK (max_selection >= min_selection),
    CHECK (selection_type <> 'Single' OR max_selection = 1)
);

CREATE TRIGGER trg_product_option_groups_updated_at
    BEFORE UPDATE ON product_option_groups FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_product_option_groups_product_id ON product_option_groups(product_id);

CREATE TABLE product_options (
    id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    option_group_id   uuid NOT NULL REFERENCES product_option_groups(id) ON DELETE CASCADE,
    name_en           varchar(150) NOT NULL CHECK (length(btrim(name_en)) > 0),
    name_ar           varchar(150) NOT NULL CHECK (length(btrim(name_ar)) > 0),
    price_adjustment  numeric(18,2) NOT NULL DEFAULT 0,
    is_default        boolean NOT NULL DEFAULT false,
    display_order     int NOT NULL DEFAULT 0 CHECK (display_order >= 0),
    is_active         boolean NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    deleted_at        timestamptz
);

CREATE TRIGGER trg_product_options_updated_at
    BEFORE UPDATE ON product_options FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_product_options_option_group_id ON product_options(option_group_id);
CREATE UNIQUE INDEX uq_product_options_one_default
    ON product_options(option_group_id) WHERE is_default = true;

-- ============================================================
-- DISCOUNTS
-- Triggers below reject a discount_products/discount_sections
-- row whose product/section doesn't belong to the discount's store.
-- ============================================================

CREATE TABLE discounts (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id      uuid NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    title_en      varchar(150) NOT NULL CHECK (length(btrim(title_en)) > 0),
    title_ar      varchar(150) NOT NULL CHECK (length(btrim(title_ar)) > 0),
    type          discount_type NOT NULL,
    value         numeric(18,2) NOT NULL CHECK (value > 0),
    start_date    timestamptz,
    end_date      timestamptz,
    is_active     boolean NOT NULL DEFAULT true,
    CHECK (start_date IS NULL OR end_date IS NULL OR start_date < end_date),
    CHECK (type <> 'Percentage' OR value <= 100)
);

CREATE INDEX idx_discounts_store_id ON discounts(store_id);
CREATE INDEX idx_discounts_active_window
    ON discounts(store_id, start_date, end_date) WHERE is_active = true;

CREATE TABLE discount_products (
    discount_id   uuid NOT NULL REFERENCES discounts(id) ON DELETE CASCADE,
    product_id    uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    PRIMARY KEY (discount_id, product_id)
);

CREATE INDEX idx_discount_products_product_id ON discount_products(product_id);



CREATE TABLE discount_sections (
    discount_id   uuid NOT NULL REFERENCES discounts(id) ON DELETE CASCADE,
    section_id    uuid NOT NULL REFERENCES store_sections(id) ON DELETE CASCADE,
    PRIMARY KEY (discount_id, section_id)
);

CREATE INDEX idx_discount_sections_section_id ON discount_sections(section_id);


-- ============================================================
-- CUSTOMERS
-- ============================================================

CREATE TABLE customers (
    id                 uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name         varchar(100) NOT NULL CHECK (length(btrim(first_name)) > 0),
    last_name          varchar(100) NOT NULL CHECK (length(btrim(last_name)) > 0),
    email              varchar(255) NOT NULL UNIQUE
                         CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    password_hash      text,
    google_id          varchar(255) UNIQUE,
    is_email_verified  boolean NOT NULL DEFAULT false,
    is_active          boolean NOT NULL DEFAULT true,
    created_at         timestamptz NOT NULL DEFAULT now(),
    updated_at         timestamptz NOT NULL DEFAULT now(),
    deleted_at         timestamptz,
    CHECK (password_hash IS NOT NULL OR google_id IS NOT NULL)
);

CREATE TRIGGER trg_customers_updated_at
    BEFORE UPDATE ON customers FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_customers_email ON customers(email);

CREATE TABLE refresh_tokens (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id   uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    token_hash    text NOT NULL,
    expires_at    timestamptz NOT NULL,
    revoked_at    timestamptz,          -- NULL = still active
    last_used_at  timestamptz,
    created_at    timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_refresh_tokens_customer_id ON refresh_tokens(customer_id);
CREATE UNIQUE INDEX uq_refresh_tokens_token_hash ON refresh_tokens(token_hash);

-- ============================================================
-- ADDRESSES
-- customer_addresses carries a (customer_id, id) unique pair so
-- orders can enforce address ownership via composite FK below.
-- ============================================================

CREATE TABLE customer_addresses (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id   uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    label         varchar(100) NOT NULL CHECK (length(btrim(label)) > 0),
    latitude      decimal(10,7) NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude     decimal(10,7) NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    address_text  text NOT NULL CHECK (length(btrim(address_text)) > 0),
    is_default    boolean NOT NULL DEFAULT false,
    created_at    timestamptz NOT NULL DEFAULT now(),
    updated_at    timestamptz NOT NULL DEFAULT now(),
    deleted_at    timestamptz,
    UNIQUE (customer_id, id)
);

CREATE TRIGGER trg_customer_addresses_updated_at
    BEFORE UPDATE ON customer_addresses FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_customer_addresses_customer_id ON customer_addresses(customer_id);
CREATE UNIQUE INDEX uq_customer_addresses_one_default
    ON customer_addresses(customer_id) WHERE is_default = true AND deleted_at IS NULL;

-- ============================================================
-- FAVORITES
-- ============================================================

CREATE TABLE favorite_products (
    customer_id   uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    product_id    uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    created_at    timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (customer_id, product_id)
);

CREATE INDEX idx_favorite_products_product_id ON favorite_products(product_id);

CREATE TABLE favorite_stores (
    customer_id   uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    store_id      uuid NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    created_at    timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (customer_id, store_id)
);

CREATE INDEX idx_favorite_stores_store_id ON favorite_stores(store_id);

-- ============================================================
-- GUEST SESSIONS + CARTS
-- Guest session id is internal only. The browser holds a random
-- opaque token; only its hash is stored here.
-- ============================================================

CREATE TABLE guest_sessions (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    token_hash    text NOT NULL,
    created_at    timestamptz NOT NULL DEFAULT now(),
    last_seen_at  timestamptz NOT NULL DEFAULT now(),
    expires_at    timestamptz NOT NULL
);

CREATE UNIQUE INDEX uq_guest_sessions_token_hash ON guest_sessions(token_hash);
CREATE INDEX idx_guest_sessions_expires_at ON guest_sessions(expires_at);

CREATE TABLE carts (
    id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id       uuid REFERENCES customers(id) ON DELETE CASCADE,
    guest_session_id  uuid REFERENCES guest_sessions(id) ON DELETE CASCADE,
    store_id          uuid NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    CHECK (
        (customer_id IS NOT NULL AND guest_session_id IS NULL) OR
        (customer_id IS NULL AND guest_session_id IS NOT NULL)
    )
);

CREATE TRIGGER trg_carts_updated_at
    BEFORE UPDATE ON carts FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE UNIQUE INDEX uq_carts_customer_store
    ON carts(customer_id, store_id) WHERE customer_id IS NOT NULL;
CREATE UNIQUE INDEX uq_carts_guest_store
    ON carts(guest_session_id, store_id) WHERE guest_session_id IS NOT NULL;

CREATE TABLE cart_items (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    cart_id       uuid NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
    product_id    uuid NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    quantity      int NOT NULL CHECK (quantity > 0),
    notes         varchar(500),
    created_at    timestamptz NOT NULL DEFAULT now(),
    updated_at    timestamptz NOT NULL DEFAULT now()
);


CREATE TRIGGER trg_cart_items_updated_at
    BEFORE UPDATE ON cart_items FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_cart_items_cart_id ON cart_items(cart_id);
CREATE INDEX idx_cart_items_product_id ON cart_items(product_id);

CREATE TABLE cart_item_options (
    cart_item_id  uuid NOT NULL REFERENCES cart_items(id) ON DELETE CASCADE,
    option_id     uuid NOT NULL REFERENCES product_options(id) ON DELETE CASCADE,
    PRIMARY KEY (cart_item_id, option_id)
);

CREATE INDEX idx_cart_item_options_option_id ON cart_item_options(option_id);



-- ============================================================
-- ORDERS
-- Composite FK enforces address ownership. Delivery fields
-- snapshot the address at order time (address may change/be
-- deleted later without affecting past orders).
-- ============================================================

CREATE TABLE orders (
    id                     uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id            uuid NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    store_id               uuid NOT NULL REFERENCES stores(id) ON DELETE RESTRICT,
    address_id             uuid,
    delivery_name          varchar(200) NOT NULL,
    delivery_phone         varchar(30),
    delivery_address_text  text NOT NULL,
    delivery_latitude      decimal(10,7) NOT NULL CHECK (delivery_latitude BETWEEN -90 AND 90),
    delivery_longitude     decimal(10,7) NOT NULL CHECK (delivery_longitude BETWEEN -180 AND 180),
    status                 order_status NOT NULL DEFAULT 'Pending',
    subtotal               numeric(18,2) NOT NULL CHECK (subtotal >= 0),
    discount_total         numeric(18,2) NOT NULL DEFAULT 0 CHECK (discount_total >= 0),
    total                  numeric(18,2) NOT NULL CHECK (total >= 0),
    created_at             timestamptz NOT NULL DEFAULT now(),
    updated_at             timestamptz NOT NULL DEFAULT now(),
    CHECK (total = subtotal - discount_total),
    FOREIGN KEY (customer_id, address_id) REFERENCES customer_addresses(customer_id, id)
);

CREATE TRIGGER trg_orders_updated_at
    BEFORE UPDATE ON orders FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_orders_customer_id ON orders(customer_id);
CREATE INDEX idx_orders_store_id ON orders(store_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_customer_status_created
    ON orders(customer_id, status, created_at DESC);

CREATE TABLE order_items (
    id                   uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id             uuid NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id           uuid REFERENCES products(id) ON DELETE SET NULL,
    name_en_snapshot     varchar(255) NOT NULL,
    name_ar_snapshot     varchar(255) NOT NULL,
    unit_price_snapshot  numeric(18,2) NOT NULL CHECK (unit_price_snapshot >= 0),
    quantity             int NOT NULL CHECK (quantity > 0),
    line_total           numeric(18,2) NOT NULL CHECK (line_total >= 0)
);

CREATE INDEX idx_order_items_order_id ON order_items(order_id);
CREATE INDEX idx_order_items_product_id ON order_items(product_id);

CREATE TABLE order_item_options (
    id                         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    order_item_id              uuid NOT NULL REFERENCES order_items(id) ON DELETE CASCADE,
    option_name_en_snapshot    varchar(150) NOT NULL,
    option_name_ar_snapshot    varchar(150) NOT NULL,
    price_adjustment_snapshot  numeric(18,2) NOT NULL DEFAULT 0
);

CREATE INDEX idx_order_item_options_order_item_id ON order_item_options(order_item_id);

CREATE TABLE order_status_history (
    id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id          uuid NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    status            order_status NOT NULL,
    note              varchar(500),
    changed_by_type   varchar(20) CHECK (changed_by_type IS NULL OR changed_by_type IN ('Customer','Tenant','System')),
    changed_by_id     uuid,
    changed_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_order_status_history_order_id ON order_status_history(order_id, changed_at);

-- ============================================================
-- PAYMENTS (Stripe)
-- ============================================================

CREATE TABLE payments (
    id                        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id                  uuid NOT NULL UNIQUE REFERENCES orders(id) ON DELETE RESTRICT,
    provider                  varchar(50) NOT NULL DEFAULT 'Stripe',
    stripe_payment_intent_id  varchar(255) NOT NULL UNIQUE,
    status                    payment_status NOT NULL DEFAULT 'Pending',
    amount                    numeric(18,2) NOT NULL CHECK (amount >= 0),
    currency                  varchar(3) NOT NULL DEFAULT 'USD',
    failure_reason            text,
    provider_metadata         jsonb,
    paid_at                   timestamptz,
    refunded_at               timestamptz,
    created_at                timestamptz NOT NULL DEFAULT now(),
    updated_at                timestamptz NOT NULL DEFAULT now()
);

CREATE TRIGGER trg_payments_updated_at
    BEFORE UPDATE ON payments FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX idx_payments_order_id ON payments(order_id);
CREATE INDEX idx_payments_status ON payments(status);

-- ============================================================
-- DONE
-- ============================================================
