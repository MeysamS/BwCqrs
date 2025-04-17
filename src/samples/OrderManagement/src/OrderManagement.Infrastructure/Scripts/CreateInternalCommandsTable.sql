-- Create internal_commands table
CREATE TABLE IF NOT EXISTS internal_commands (
    "Id" UUID PRIMARY KEY,
    "Type" VARCHAR(500) NOT NULL,
    "Data" TEXT NOT NULL,
    "ScheduledOn" TIMESTAMP NOT NULL,
    "ProcessedOn" TIMESTAMP,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "Error" VARCHAR(2000),
    "Status" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "LastRetryAt" TIMESTAMP
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_internal_commands_status ON internal_commands("Status");
CREATE INDEX IF NOT EXISTS idx_internal_commands_scheduled_on ON internal_commands("ScheduledOn");
CREATE INDEX IF NOT EXISTS idx_internal_commands_processed_on ON internal_commands("ProcessedOn"); 