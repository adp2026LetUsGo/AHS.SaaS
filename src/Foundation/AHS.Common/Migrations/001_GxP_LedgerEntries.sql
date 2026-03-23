-- src/Foundation/AHS.Common/Migrations/001_GxP_LedgerEntries.sql
-- Run once per Cell database during Cell onboarding

CREATE TABLE IF NOT EXISTS ledger_entries (
    sequence       BIGSERIAL    NOT NULL,
    tenant_id      UUID         NOT NULL,
    aggregate_id   UUID         NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type     VARCHAR(200) NOT NULL,
    payload_json   JSONB        NOT NULL,
    actor_id       VARCHAR(100) NOT NULL,
    actor_name     VARCHAR(200) NOT NULL,
    occurred_at    TIMESTAMPTZ  NOT NULL,
    previous_hash  CHAR(64)     NOT NULL,
    entry_hash     CHAR(64)     NOT NULL,
    hmac_seal      VARCHAR(88)  NOT NULL,
    CONSTRAINT pk_ledger_entries PRIMARY KEY (tenant_id, sequence),
    CONSTRAINT uq_ledger_entry_hash UNIQUE (tenant_id, entry_hash)
);

ALTER TABLE ledger_entries ENABLE ROW LEVEL SECURITY;

-- Assuming current_setting('app.current_tenant_id') is set by interceptor
CREATE POLICY ledger_tenant_isolation ON ledger_entries
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

-- REVOKE UPDATE, DELETE ON ledger_entries FROM app_role; -- Uncomment if role exists

CREATE INDEX ix_ledger_aggregate
    ON ledger_entries (tenant_id, aggregate_id, sequence);
