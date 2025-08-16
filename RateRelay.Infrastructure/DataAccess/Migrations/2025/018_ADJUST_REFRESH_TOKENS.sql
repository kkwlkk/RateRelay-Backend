ALTER TABLE refresh_tokens
    DROP FOREIGN KEY FK_refresh_tokens_accounts_AccountId;

ALTER TABLE refresh_tokens
    ADD CONSTRAINT FK_refresh_tokens_accounts_AccountId
        FOREIGN KEY (AccountId) REFERENCES accounts (Id) ON DELETE CASCADE;