CREATE TABLE IF NOT EXISTS account_bans
(
    id              BIGINT       NOT NULL AUTO_INCREMENT,
    AccountId       BIGINT       NOT NULL,
    ExpiresAtUtc    DATETIME     NULL,
    Reason          VARCHAR(255) NOT NULL,
    DateCreatedUtc  DATETIME     NOT NULL,
    DateModifiedUtc DATETIME     NULL,
    DateDeletedUtc  DATETIME     NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (AccountId) REFERENCES accounts (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_account_bans_account_id ON account_bans (AccountId);
CREATE INDEX IF NOT EXISTS idx_account_bans_expires_at_utc ON account_bans (ExpiresAtUtc);
CREATE INDEX IF NOT EXISTS idx_account_bans_date_created_utc ON account_bans (DateCreatedUtc);