ALTER TABLE `account_bans` RENAME COLUMN `id` TO `Id`;

CREATE TABLE IF NOT EXISTS `maintenance_mode`
(
    id                 BIGINT       NOT NULL AUTO_INCREMENT,
    IsActive           BOOLEAN      NOT NULL,
    Reason             VARCHAR(500) NULL,
    CreatedByAccountId BIGINT       NOT NULL,
    DateCreatedUtc     DATETIME     NOT NULL,
    DateModifiedUtc    DATETIME     NULL,
    DateDeletedUtc     DATETIME     NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (CreatedByAccountId) REFERENCES accounts (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_maintenance_mode_is_active ON maintenance_mode (IsActive);
CREATE INDEX IF NOT EXISTS idx_maintenance_mode_created_by_account_id ON maintenance_mode (CreatedByAccountId);