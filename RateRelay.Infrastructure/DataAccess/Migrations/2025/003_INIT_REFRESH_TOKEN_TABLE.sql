CREATE TABLE refresh_tokens
(
    Id              BIGINT       NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Token           VARCHAR(255) NOT NULL UNIQUE,
    ExpirationDate  DATETIME     NOT NULL,
    AccountId       BIGINT       NOT NULL,
    DateCreatedUtc  DATETIME     NOT NULL,
    DateModifiedUtc DATETIME     NULL,
    DateDeletedUtc  DATETIME     NULL
);

CREATE UNIQUE INDEX IX_refresh_tokens_Token ON refresh_tokens (Token);

ALTER TABLE refresh_tokens
    ADD CONSTRAINT FK_refresh_tokens_accounts_AccountId
        FOREIGN KEY (AccountId) REFERENCES accounts (Id);