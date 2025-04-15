CREATE TABLE accounts
(
    Id              BIGINT       NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Username        VARCHAR(64)  NOT NULL UNIQUE,
    Email           VARCHAR(255) NOT NULL UNIQUE,
    GoogleId        VARCHAR(255) NOT NULL UNIQUE,
    DateCreatedUtc  DATETIME     NOT NULL,
    DateModifiedUtc DATETIME     NULL,
    DateDeletedUtc  DATETIME     NULL
);

CREATE UNIQUE INDEX IX_Accounts_Username ON accounts (Username);
CREATE UNIQUE INDEX IX_Accounts_Email ON accounts (Email);
CREATE UNIQUE INDEX IX_Accounts_GoogleId ON accounts (GoogleId);