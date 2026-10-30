CREATE TABLE world_sessions (
    world_session_id uuid PRIMARY KEY,
    account_id uuid NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    character_id uuid NOT NULL REFERENCES characters(character_id) ON DELETE CASCADE,
    map_id varchar(64) NOT NULL,
    spawn_id varchar(64) NOT NULL,
    position_x double precision NOT NULL,
    position_y double precision NOT NULL,
    position_z double precision NOT NULL,
    velocity_x double precision NOT NULL DEFAULT 0,
    velocity_y double precision NOT NULL DEFAULT 0,
    velocity_z double precision NOT NULL DEFAULT 0,
    last_input_sequence bigint NOT NULL DEFAULT 0 CHECK (last_input_sequence >= 0),
    last_input_tick bigint NOT NULL DEFAULT 0 CHECK (last_input_tick >= 0),
    created_at timestamptz NOT NULL DEFAULT now(),
    last_active_at timestamptz NOT NULL DEFAULT now(),
    expires_at timestamptz NOT NULL,
    closed_at timestamptz NULL,
    CONSTRAINT ck_world_sessions_position_x CHECK (position_x BETWEEN -1000 AND 1000),
    CONSTRAINT ck_world_sessions_position_y CHECK (position_y BETWEEN -1000 AND 1000),
    CONSTRAINT ck_world_sessions_position_z CHECK (position_z BETWEEN -1000 AND 1000),
    CONSTRAINT ck_world_sessions_expiry CHECK (expires_at > created_at),
    CONSTRAINT ck_world_sessions_close CHECK (closed_at IS NULL OR closed_at >= created_at)
);
CREATE UNIQUE INDEX uq_world_sessions_active_character
    ON world_sessions(character_id) WHERE closed_at IS NULL;
CREATE INDEX ix_world_sessions_owner_expiry
    ON world_sessions(account_id, expires_at) WHERE closed_at IS NULL;
