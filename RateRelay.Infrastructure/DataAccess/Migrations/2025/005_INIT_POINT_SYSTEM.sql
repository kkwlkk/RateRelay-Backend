ALTER TABLE accounts
    ADD COLUMN PointBalance INT DEFAULT 0 NOT NULL AFTER Permissions;

CREATE TABLE point_transactions
(
    Id                 bigint       NOT NULL AUTO_INCREMENT PRIMARY KEY,
    AccountId          bigint       NOT NULL,
    Amount             int          NOT NULL,
    TransactionType    varchar(50)  NOT NULL,
    Description        varchar(255) NULL,
    TransactionDateUtc datetime     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateCreatedUtc     datetime     NOT NULL,
    DateModifiedUtc    datetime     NULL,
    DateDeletedUtc     datetime     NULL,
    CONSTRAINT FK_point_transactions_accounts_AccountId FOREIGN KEY (AccountId) REFERENCES accounts (Id)
);

CREATE INDEX IX_point_transactions_AccountId ON point_transactions (AccountId);
CREATE INDEX IX_point_transactions_TransactionType ON point_transactions (TransactionType);