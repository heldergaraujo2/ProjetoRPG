CREATE TABLE accounts (
    account_id uuid PRIMARY KEY,
    username varchar(32) NOT NULL,
    username_normalized varchar(32) NOT NULL,
    password_hash text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_accounts_username_normalized UNIQUE (username_normalized),
    CONSTRAINT ck_accounts_username CHECK (username ~ '^[A-Za-z0-9_.-]{3,32}$'),
    CONSTRAINT ck_accounts_username_normalized CHECK (username_normalized ~ '^[A-Z0-9_.-]{3,32}$'),
    CONSTRAINT ck_accounts_password_hash_nonempty CHECK (length(password_hash) > 0)
);

CREATE TABLE account_sessions (
    token_hash bytea PRIMARY KEY,
    account_id uuid NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    created_at timestamptz NOT NULL DEFAULT now(),
    expires_at timestamptz NOT NULL,
    revoked_at timestamptz NULL,
    CONSTRAINT ck_sessions_token_hash_length CHECK (octet_length(token_hash) = 32),
    CONSTRAINT ck_sessions_expiration CHECK (expires_at > created_at),
    CONSTRAINT ck_sessions_revocation CHECK (revoked_at IS NULL OR revoked_at >= created_at)
);
CREATE INDEX ix_account_sessions_owner_expiry ON account_sessions(account_id, expires_at);
CREATE INDEX ix_account_sessions_expiry ON account_sessions(expires_at);

CREATE TABLE characters (
    character_id uuid PRIMARY KEY,
    account_id uuid NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    name varchar(20) NOT NULL,
    name_normalized varchar(20) NOT NULL,
    class_id varchar(64) NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_characters_name_normalized UNIQUE (name_normalized),
    CONSTRAINT ck_characters_name CHECK (name ~ '^[A-Za-z][A-Za-z0-9_]{2,19}$'),
    CONSTRAINT ck_characters_name_normalized CHECK (name_normalized ~ '^[A-Z][A-Z0-9_]{2,19}$'),
    CONSTRAINT ck_characters_class_id CHECK (class_id IS NULL OR class_id ~ '^[a-z0-9]+([._][a-z0-9]+)*$')
);
CREATE INDEX ix_characters_account_created ON characters(account_id, created_at, character_id);

CREATE TABLE security_audit_events (
    event_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    event_type varchar(32) NOT NULL,
    actor_account_id uuid NULL REFERENCES accounts(account_id) ON DELETE SET NULL,
    occurred_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_audit_event_type CHECK (event_type IN (
        'account_registered', 'login_succeeded', 'login_failed',
        'session_revoked', 'character_created', 'character_renamed'
    ))
);
CREATE INDEX ix_security_audit_actor_time ON security_audit_events(actor_account_id, occurred_at DESC);
CREATE INDEX ix_security_audit_time ON security_audit_events(occurred_at DESC);
