ALTER TABLE accounts
    ADD COLUMN Permissions BIGINT UNSIGNED NOT NULL DEFAULT 0 AFTER GoogleId,
    ADD COLUMN RoleId BIGINT NULL AFTER Permissions;

CREATE TABLE roles
(
    Id              BIGINT      NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Name            VARCHAR(64) NOT NULL UNIQUE,
    Description     VARCHAR(255) NULL,
    Permissions     BIGINT UNSIGNED NOT NULL DEFAULT 0,
    is_hidden       BOOLEAN NOT NULL DEFAULT FALSE,
    DateCreatedUtc  DATETIME    NOT NULL,
    DateModifiedUtc DATETIME    NULL,
    DateDeletedUtc  DATETIME    NULL
);

CREATE UNIQUE INDEX IX_roles_Name ON roles (Name);

ALTER TABLE accounts
    ADD CONSTRAINT FK_accounts_roles_RoleId
        FOREIGN KEY (RoleId) REFERENCES roles (Id);