DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'order') THEN
        CREATE SCHEMA "order";
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'catalog') THEN
        CREATE SCHEMA catalog;
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'promotion') THEN
        CREATE SCHEMA promotion;
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'inventory') THEN
        CREATE SCHEMA inventory;
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'system') THEN
        CREATE SCHEMA system;
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'payment') THEN
        CREATE SCHEMA payment;
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'social_proof') THEN
        CREATE SCHEMA social_proof;
    END IF;
END $EF$;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'identity') THEN
        CREATE SCHEMA identity;
    END IF;
END $EF$;


CREATE TABLE catalog.categories (
    categoryid uuid NOT NULL,
    parentcategoryid uuid,
    name character varying(100) NOT NULL,
    slug character varying(150) NOT NULL,
    description text,
    displayorder integer NOT NULL DEFAULT 0,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT categories_pkey PRIMARY KEY (categoryid),
    CONSTRAINT categories_parentcategoryid_fkey FOREIGN KEY (parentcategoryid) REFERENCES catalog.categories (categoryid) ON DELETE SET NULL
);


CREATE TABLE promotion.coupons (
    couponid uuid NOT NULL,
    code character varying(50) NOT NULL,
    discounttype character varying(20) NOT NULL,
    discountvalue numeric(10,2) NOT NULL,
    minordervalue numeric(10,2) NOT NULL,
    maxdiscountamount numeric(10,2),
    usagelimit integer,
    usedcount integer NOT NULL DEFAULT 0,
    startdate timestamp with time zone NOT NULL,
    enddate timestamp with time zone NOT NULL,
    isactive boolean NOT NULL DEFAULT TRUE,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT coupons_pkey PRIMARY KEY (couponid)
);


CREATE TABLE system.systemlogs (
    logid uuid NOT NULL,
    userid uuid,
    loglevel character varying(20) NOT NULL,
    action character varying(100) NOT NULL,
    message text NOT NULL,
    ipaddress character varying(50),
    useragent text,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT systemlogs_pkey PRIMARY KEY (logid)
);


CREATE TABLE identity.users (
    userid uuid NOT NULL,
    email character varying(255) NOT NULL,
    passwordhash character varying(255),
    fullname character varying(100),
    phone character varying(20),
    avatarurl character varying(500),
    role character varying(20) NOT NULL DEFAULT ('Customer'::character varying),
    identityid character varying(255),
    isemailverified boolean NOT NULL DEFAULT FALSE,
    isactive boolean NOT NULL DEFAULT TRUE,
    fcmtoken character varying(500),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT users_pkey PRIMARY KEY (userid)
);


CREATE TABLE catalog.products (
    productid uuid NOT NULL,
    categoryid uuid NOT NULL,
    name character varying(200) NOT NULL,
    slug character varying(250) NOT NULL,
    description text,
    fittype character varying(50),
    gender character varying(20) NOT NULL DEFAULT ('Unisex'::character varying),
    isactive boolean NOT NULL DEFAULT TRUE,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT products_pkey PRIMARY KEY (productid),
    CONSTRAINT products_categoryid_fkey FOREIGN KEY (categoryid) REFERENCES catalog.categories (categoryid) ON DELETE RESTRICT
);


CREATE TABLE "order".carts (
    cartid uuid NOT NULL,
    userid uuid,
    sessionid character varying(100),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT carts_pkey PRIMARY KEY (cartid),
    CONSTRAINT carts_userid_fkey FOREIGN KEY (userid) REFERENCES identity.users (userid) ON DELETE CASCADE
);


CREATE TABLE system.notifications (
    notificationid uuid NOT NULL,
    userid uuid NOT NULL,
    title character varying(200) NOT NULL,
    content text NOT NULL,
    type character varying(50) NOT NULL,
    isread boolean NOT NULL DEFAULT FALSE,
    targeturl character varying(500),
    readat timestamp with time zone,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT notifications_pkey PRIMARY KEY (notificationid),
    CONSTRAINT notifications_userid_fkey FOREIGN KEY (userid) REFERENCES identity.users (userid) ON DELETE CASCADE
);


CREATE TABLE "order".orders (
    orderid uuid NOT NULL,
    ordercode character varying(30) NOT NULL,
    userid uuid,
    shippingaddress text NOT NULL,
    subtotal numeric(10,2) NOT NULL,
    shippingfee numeric(10,2) NOT NULL,
    discountamount numeric(10,2) NOT NULL,
    totalamount numeric(10,2) NOT NULL,
    currentstatus character varying(30) NOT NULL,
    paymentmethod character varying(20) NOT NULL,
    paymentstatus character varying(20) NOT NULL DEFAULT ('Unpaid'::character varying),
    customernote text,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT orders_pkey PRIMARY KEY (orderid),
    CONSTRAINT orders_userid_fkey FOREIGN KEY (userid) REFERENCES identity.users (userid) ON DELETE RESTRICT
);


CREATE TABLE identity.refreshtokens (
    refreshtokenid uuid NOT NULL,
    userid uuid NOT NULL,
    token character varying(500) NOT NULL,
    jwtid character varying(255) NOT NULL,
    isused boolean NOT NULL DEFAULT FALSE,
    isrevoked boolean NOT NULL DEFAULT FALSE,
    expirydate timestamp with time zone NOT NULL,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT refreshtokens_pkey PRIMARY KEY (refreshtokenid),
    CONSTRAINT refreshtokens_userid_fkey FOREIGN KEY (userid) REFERENCES identity.users (userid) ON DELETE CASCADE
);


CREATE TABLE identity.useraddresses (
    addressid uuid NOT NULL,
    userid uuid NOT NULL,
    receivername character varying(100) NOT NULL,
    phonenumber character varying(20) NOT NULL,
    addressline1 character varying(255) NOT NULL,
    ward character varying(100) NOT NULL,
    district character varying(100) NOT NULL,
    city character varying(100) NOT NULL,
    isdefault boolean NOT NULL DEFAULT FALSE,
    addresstype character varying(20) NOT NULL DEFAULT ('Home'::character varying),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT useraddresses_pkey PRIMARY KEY (addressid),
    CONSTRAINT useraddresses_userid_fkey FOREIGN KEY (userid) REFERENCES identity.users (userid) ON DELETE CASCADE
);


CREATE TABLE catalog.productvariants (
    variantid uuid NOT NULL,
    productid uuid NOT NULL,
    sku character varying(50) NOT NULL,
    colorname character varying(50) NOT NULL,
    colorhex character varying(10),
    size character varying(10) NOT NULL,
    price numeric(10,2) NOT NULL,
    originalprice numeric(10,2),
    weightgrams integer,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT productvariants_pkey PRIMARY KEY (variantid),
    CONSTRAINT productvariants_productid_fkey FOREIGN KEY (productid) REFERENCES catalog.products (productid) ON DELETE CASCADE
);


CREATE TABLE catalog.sizeguides (
    guideid uuid NOT NULL,
    productid uuid NOT NULL,
    size character varying(10) NOT NULL,
    chestcm character varying(50),
    waistcm character varying(50),
    hipscm character varying(50),
    heightrangecm character varying(50),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT sizeguides_pkey PRIMARY KEY (guideid),
    CONSTRAINT sizeguides_productid_fkey FOREIGN KEY (productid) REFERENCES catalog.products (productid) ON DELETE CASCADE
);


CREATE TABLE social_proof.wishlists (
    wishlistid uuid NOT NULL,
    userid uuid NOT NULL,
    productid uuid NOT NULL,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT wishlists_pkey PRIMARY KEY (wishlistid),
    CONSTRAINT wishlists_productid_fkey FOREIGN KEY (productid) REFERENCES catalog.products (productid) ON DELETE CASCADE,
    CONSTRAINT wishlists_userid_fkey FOREIGN KEY (userid) REFERENCES identity.users (userid) ON DELETE CASCADE
);


CREATE TABLE promotion.couponusages (
    usageid uuid NOT NULL,
    couponid uuid NOT NULL,
    userid uuid NOT NULL,
    orderid uuid NOT NULL,
    usedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT couponusages_pkey PRIMARY KEY (usageid),
    CONSTRAINT couponusages_couponid_fkey FOREIGN KEY (couponid) REFERENCES promotion.coupons (couponid) ON DELETE CASCADE,
    CONSTRAINT couponusages_orderid_fkey FOREIGN KEY (orderid) REFERENCES "order".orders (orderid) ON DELETE CASCADE,
    CONSTRAINT couponusages_userid_fkey FOREIGN KEY (userid) REFERENCES identity.users (userid) ON DELETE CASCADE
);


CREATE TABLE "order".ordertrackinghistories (
    trackingid uuid NOT NULL,
    orderid uuid NOT NULL,
    status character varying(30) NOT NULL,
    title character varying(150) NOT NULL,
    description text,
    location character varying(150),
    timestamp timestamp with time zone NOT NULL,
    updatedby character varying(50),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT ordertrackinghistories_pkey PRIMARY KEY (trackingid),
    CONSTRAINT ordertrackinghistories_orderid_fkey FOREIGN KEY (orderid) REFERENCES "order".orders (orderid) ON DELETE CASCADE
);


CREATE TABLE payment.paymenttransactions (
    transactionid uuid NOT NULL,
    orderid uuid NOT NULL,
    gateway character varying(50) NOT NULL,
    gatewaytransactionid character varying(100),
    amount numeric(10,2) NOT NULL,
    status character varying(20) NOT NULL,
    paymentdate timestamp with time zone,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT paymenttransactions_pkey PRIMARY KEY (transactionid),
    CONSTRAINT paymenttransactions_orderid_fkey FOREIGN KEY (orderid) REFERENCES "order".orders (orderid) ON DELETE RESTRICT
);


CREATE TABLE social_proof.productreviews (
    reviewid uuid NOT NULL,
    productid uuid NOT NULL,
    userid uuid NOT NULL,
    orderid uuid NOT NULL,
    rating integer NOT NULL,
    comment text,
    fitfeedback character varying(20),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT productreviews_pkey PRIMARY KEY (reviewid),
    CONSTRAINT productreviews_orderid_fkey FOREIGN KEY (orderid) REFERENCES "order".orders (orderid) ON DELETE RESTRICT,
    CONSTRAINT productreviews_productid_fkey FOREIGN KEY (productid) REFERENCES catalog.products (productid) ON DELETE CASCADE,
    CONSTRAINT productreviews_userid_fkey FOREIGN KEY (userid) REFERENCES identity.users (userid) ON DELETE RESTRICT
);


CREATE TABLE "order".cartitems (
    cartitemid uuid NOT NULL,
    cartid uuid NOT NULL,
    variantid uuid NOT NULL,
    quantity integer NOT NULL,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT cartitems_pkey PRIMARY KEY (cartitemid),
    CONSTRAINT cartitems_cartid_fkey FOREIGN KEY (cartid) REFERENCES "order".carts (cartid) ON DELETE CASCADE,
    CONSTRAINT cartitems_variantid_fkey FOREIGN KEY (variantid) REFERENCES catalog.productvariants (variantid) ON DELETE CASCADE
);


CREATE TABLE inventory.inventoryitems (
    inventoryid uuid NOT NULL,
    variantid uuid NOT NULL,
    quantityonhand integer NOT NULL DEFAULT 0,
    quantityreserved integer NOT NULL DEFAULT 0,
    safetystock integer NOT NULL DEFAULT 5,
    rowversion integer NOT NULL DEFAULT 1,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT inventoryitems_pkey PRIMARY KEY (inventoryid),
    CONSTRAINT inventoryitems_variantid_fkey FOREIGN KEY (variantid) REFERENCES catalog.productvariants (variantid) ON DELETE CASCADE
);


CREATE TABLE inventory.inventorytransactions (
    transactionid uuid NOT NULL,
    variantid uuid NOT NULL,
    quantitychange integer NOT NULL,
    type character varying(30) NOT NULL,
    referenceid character varying(100),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT inventorytransactions_pkey PRIMARY KEY (transactionid),
    CONSTRAINT inventorytransactions_variantid_fkey FOREIGN KEY (variantid) REFERENCES catalog.productvariants (variantid) ON DELETE RESTRICT
);


CREATE TABLE "order".orderitems (
    orderitemid uuid NOT NULL,
    orderid uuid NOT NULL,
    variantid uuid NOT NULL,
    sku character varying(50) NOT NULL,
    productname character varying(255) NOT NULL,
    unitprice numeric(10,2) NOT NULL,
    quantity integer NOT NULL,
    totalprice numeric(10,2) NOT NULL,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT orderitems_pkey PRIMARY KEY (orderitemid),
    CONSTRAINT orderitems_orderid_fkey FOREIGN KEY (orderid) REFERENCES "order".orders (orderid) ON DELETE CASCADE,
    CONSTRAINT orderitems_variantid_fkey FOREIGN KEY (variantid) REFERENCES catalog.productvariants (variantid) ON DELETE RESTRICT
);


CREATE TABLE catalog.productimages (
    imageid uuid NOT NULL,
    productid uuid NOT NULL,
    variantid uuid,
    imageurl character varying(500) NOT NULL,
    displayorder integer NOT NULL DEFAULT 0,
    isprimary boolean NOT NULL DEFAULT FALSE,
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT productimages_pkey PRIMARY KEY (imageid),
    CONSTRAINT productimages_productid_fkey FOREIGN KEY (productid) REFERENCES catalog.products (productid) ON DELETE CASCADE,
    CONSTRAINT productimages_variantid_fkey FOREIGN KEY (variantid) REFERENCES catalog.productvariants (variantid) ON DELETE SET NULL
);


CREATE TABLE social_proof.reviewmedias (
    mediaid uuid NOT NULL,
    reviewid uuid NOT NULL,
    mediaurl character varying(500) NOT NULL,
    mediatype character varying(20) NOT NULL DEFAULT ('Image'::character varying),
    createdat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updatedat timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    deletedat timestamp with time zone,
    CONSTRAINT reviewmedias_pkey PRIMARY KEY (mediaid),
    CONSTRAINT reviewmedias_reviewid_fkey FOREIGN KEY (reviewid) REFERENCES social_proof.productreviews (reviewid) ON DELETE CASCADE
);


CREATE INDEX "IX_cartitems_cartid" ON "order".cartitems (cartid);


CREATE INDEX "IX_cartitems_variantid" ON "order".cartitems (variantid);


CREATE INDEX "IX_carts_userid" ON "order".carts (userid);


CREATE UNIQUE INDEX categories_slug_key ON catalog.categories (slug);


CREATE INDEX "IX_categories_parentcategoryid" ON catalog.categories (parentcategoryid);


CREATE UNIQUE INDEX coupons_code_key ON promotion.coupons (code);


CREATE INDEX "IX_couponusages_couponid" ON promotion.couponusages (couponid);


CREATE INDEX "IX_couponusages_orderid" ON promotion.couponusages (orderid);


CREATE INDEX "IX_couponusages_userid" ON promotion.couponusages (userid);


CREATE INDEX idx_inventory_variant ON inventory.inventoryitems (variantid);


CREATE UNIQUE INDEX inventoryitems_variantid_key ON inventory.inventoryitems (variantid);


CREATE INDEX "IX_inventorytransactions_variantid" ON inventory.inventorytransactions (variantid);


CREATE INDEX "IX_notifications_userid" ON system.notifications (userid);


CREATE INDEX "IX_orderitems_orderid" ON "order".orderitems (orderid);


CREATE INDEX "IX_orderitems_variantid" ON "order".orderitems (variantid);


CREATE INDEX idx_orders_code ON "order".orders (ordercode);


CREATE INDEX idx_orders_user ON "order".orders (userid) WHERE (deletedat IS NULL);


CREATE UNIQUE INDEX orders_ordercode_key ON "order".orders (ordercode);


CREATE INDEX idx_tracking_order ON "order".ordertrackinghistories (orderid);


CREATE INDEX "IX_paymenttransactions_orderid" ON payment.paymenttransactions (orderid);


CREATE INDEX "IX_productimages_productid" ON catalog.productimages (productid);


CREATE INDEX "IX_productimages_variantid" ON catalog.productimages (variantid);


CREATE INDEX "IX_productreviews_orderid" ON social_proof.productreviews (orderid);


CREATE INDEX "IX_productreviews_productid" ON social_proof.productreviews (productid);


CREATE INDEX "IX_productreviews_userid" ON social_proof.productreviews (userid);


CREATE INDEX idx_products_category ON catalog.products (categoryid) WHERE (deletedat IS NULL);


CREATE UNIQUE INDEX products_slug_key ON catalog.products (slug);


CREATE INDEX idx_variants_product ON catalog.productvariants (productid) WHERE (deletedat IS NULL);


CREATE UNIQUE INDEX productvariants_sku_key ON catalog.productvariants (sku);


CREATE INDEX "IX_refreshtokens_userid" ON identity.refreshtokens (userid);


CREATE UNIQUE INDEX refreshtokens_token_key ON identity.refreshtokens (token);


CREATE INDEX "IX_reviewmedias_reviewid" ON social_proof.reviewmedias (reviewid);


CREATE INDEX "IX_sizeguides_productid" ON catalog.sizeguides (productid);


CREATE INDEX "IX_useraddresses_userid" ON identity.useraddresses (userid);


CREATE UNIQUE INDEX users_email_key ON identity.users (email);


CREATE UNIQUE INDEX users_identityid_key ON identity.users (identityid);


CREATE INDEX "IX_wishlists_productid" ON social_proof.wishlists (productid);


CREATE UNIQUE INDEX uq_wishlist_user_product ON social_proof.wishlists (userid, productid);


