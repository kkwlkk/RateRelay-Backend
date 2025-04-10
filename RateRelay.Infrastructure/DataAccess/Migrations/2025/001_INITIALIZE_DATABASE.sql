CREATE TABLE accounts
(
    Id              BIGINT            NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Username        VARCHAR(64)       NOT NULL UNIQUE,
    PasswordHash    VARCHAR(255)      NOT NULL,
    DateCreatedUtc  DATETIME          NOT NULL,
    DateModifiedUtc DATETIME          NULL,
    DateDeletedUtc  DATETIME          NULL
);

CREATE UNIQUE INDEX IX_accounts_Username ON accounts (Username);