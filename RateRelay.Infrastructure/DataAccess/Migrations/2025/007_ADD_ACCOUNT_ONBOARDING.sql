ALTER TABLE accounts
    ADD COLUMN OnboardingStep TINYINT NOT NULL DEFAULT 0,
    ADD COLUMN OnboardingLastUpdatedUtc DATETIME NULL;

CREATE INDEX IX_accounts_OnboardingStep ON accounts (OnboardingStep);