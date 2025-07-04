ALTER TABLE accounts
    ADD COLUMN ReferralCode VARCHAR(16) UNIQUE NULL AFTER PointBalance,
    ADD COLUMN ReferredByAccountId BIGINT NULL AFTER ReferralCode;

ALTER TABLE accounts
    ADD CONSTRAINT FK_accounts_referred_by
        FOREIGN KEY (ReferredByAccountId) REFERENCES accounts (Id);

CREATE INDEX IX_accounts_ReferralCode ON accounts (ReferralCode);
CREATE INDEX IX_accounts_ReferredByAccountId ON accounts (ReferredByAccountId);

CREATE TABLE referral_goals
(
    Id              BIGINT       NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Name            VARCHAR(64)  NOT NULL,
    Description     VARCHAR(255) NOT NULL,
    GoalType        TINYINT      NOT NULL,
    TargetValue     INT          NOT NULL DEFAULT 1,
    RewardPoints    INT          NOT NULL DEFAULT 0,
    IsActive        BOOLEAN      NOT NULL DEFAULT TRUE,
    SortOrder       INT          NOT NULL DEFAULT 0,
    DateCreatedUtc  DATETIME     NOT NULL,
    DateModifiedUtc DATETIME     NULL,
    DateDeletedUtc  DATETIME     NULL
);

CREATE TABLE referral_progress
(
    Id                 BIGINT   NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ReferrerAccountId  BIGINT   NOT NULL,
    ReferredAccountId  BIGINT   NOT NULL,
    GoalId             BIGINT   NOT NULL,
    CurrentValue       INT      NOT NULL DEFAULT 0,
    IsCompleted        BOOLEAN  NOT NULL DEFAULT FALSE,
    DateCompletedUtc   DATETIME NULL,
    DateCreatedUtc     DATETIME NOT NULL,
    DateModifiedUtc    DATETIME NULL,
    DateDeletedUtc     DATETIME NULL,

    CONSTRAINT FK_referral_progress_referrer
        FOREIGN KEY (ReferrerAccountId) REFERENCES accounts (Id),
    CONSTRAINT FK_referral_progress_referred
        FOREIGN KEY (ReferredAccountId) REFERENCES accounts (Id),
    CONSTRAINT FK_referral_progress_goal
        FOREIGN KEY (GoalId) REFERENCES referral_goals (Id),

    UNIQUE KEY UK_referral_progress_unique (ReferrerAccountId, ReferredAccountId, GoalId)
);

CREATE TABLE referral_rewards
(
    Id                 BIGINT   NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ReferrerAccountId  BIGINT   NOT NULL,
    ReferredAccountId  BIGINT   NOT NULL,
    GoalId             BIGINT   NOT NULL,
    RewardPoints       INT      NOT NULL,
    DateAwardedUtc     DATETIME NOT NULL,
    DateCreatedUtc     DATETIME NOT NULL,
    DateModifiedUtc    DATETIME NULL,
    DateDeletedUtc     DATETIME NULL,

    CONSTRAINT FK_referral_rewards_referrer
        FOREIGN KEY (ReferrerAccountId) REFERENCES accounts (Id),
    CONSTRAINT FK_referral_rewards_referred
        FOREIGN KEY (ReferredAccountId) REFERENCES accounts (Id),
    CONSTRAINT FK_referral_rewards_goal
        FOREIGN KEY (GoalId) REFERENCES referral_goals (Id)
);

CREATE INDEX IX_referral_progress_referrer ON referral_progress (ReferrerAccountId);
CREATE INDEX IX_referral_progress_referred ON referral_progress (ReferredAccountId);
CREATE INDEX IX_referral_progress_goal ON referral_progress (GoalId);
CREATE INDEX IX_referral_progress_completed ON referral_progress (IsCompleted, DateCompletedUtc);

CREATE INDEX IX_referral_rewards_referrer ON referral_rewards (ReferrerAccountId);
CREATE INDEX IX_referral_rewards_referred ON referral_rewards (ReferredAccountId);
CREATE INDEX IX_referral_rewards_date ON referral_rewards (DateAwardedUtc);

INSERT INTO referral_goals (Name, Description, GoalType, TargetValue, RewardPoints, IsActive, SortOrder, DateCreatedUtc)
VALUES
    ('First Review', 'Referred user completes their first business review', 0, 1, 2, TRUE, 1, NOW()),
    ('Business Verification', 'Referred user verifies their business', 1, 1, 2, TRUE, 2, NOW()),
    ('Review Milestone', 'Referred user completes 5 business reviews', 0, 5, 15, TRUE, 3, NOW()),
    ('Point Earner', 'Referred user earns 50 points', 2, 50, 10, TRUE, 4, NOW()),
    ('Active User', 'Referred user completes 10 business reviews', 0, 10, 25, TRUE, 5, NOW());