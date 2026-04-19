-- Migration: 002_Xinfer_OutboxPattern.sql
-- Goal: Atomic event persistence for guaranteed delivery
-- Cell: Xinfer

CREATE TABLE IF NOT EXISTS outbox_messages (
    id            UUID PRIMARY KEY,
    event_type    TEXT NOT NULL,
    payload_json  JSONB NOT NULL,
    occurred_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at  TIMESTAMPTZ NULL,
    error         TEXT NULL
);

-- Index for background worker performance
CREATE INDEX IF NOT EXISTS idx_outbox_unprocessed 
ON outbox_messages (occurred_at) 
WHERE processed_at IS NULL;
