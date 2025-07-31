CREATE TABLE IF NOT EXISTS business_boosts
(
    Id                  BIGINT AUTO_INCREMENT PRIMARY KEY,
    BusinessId          BIGINT       NOT NULL,
    TargetReviews       INT          NOT NULL,
    ReviewsAtBoostStart INT          NOT NULL,
    BoostedAt           DATETIME     NOT NULL,
    CompletedAt         DATETIME     NULL,
    IsActive            BOOLEAN      NOT NULL DEFAULT TRUE,
    CreatedById         BIGINT       NULL,
    CreatedBySystem     BOOLEAN      NOT NULL DEFAULT FALSE,
    Reason              VARCHAR(255) NULL,
    DateCreatedUtc      DATETIME     NOT NULL,
    DateModifiedUtc     DATETIME     NULL,
    DateDeletedUtc      DATETIME     NULL,

    FOREIGN KEY (BusinessId) REFERENCES businesses (Id),
    FOREIGN KEY (CreatedById) REFERENCES accounts (Id)
);

CREATE TABLE IF NOT EXISTS business_priority_history
(
    Id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    BusinessId      BIGINT           NOT NULL,
    OldPriority     TINYINT UNSIGNED NOT NULL,
    NewPriority     TINYINT UNSIGNED NOT NULL,
    ChangedById     BIGINT           NULL,
    ChangedBySystem BOOLEAN          NOT NULL DEFAULT FALSE,
    Reason          VARCHAR(255)     NULL,
    DateCreatedUtc  DATETIME         NOT NULL,
    DateModifiedUtc DATETIME         NULL,
    DateDeletedUtc  DATETIME         NULL,

    FOREIGN KEY (BusinessId) REFERENCES businesses (Id),
    FOREIGN KEY (ChangedById) REFERENCES accounts (Id)
);

CREATE INDEX IF NOT EXISTS IX_business_boosts_BusinessId ON business_boosts (BusinessId);
CREATE INDEX IF NOT EXISTS IX_business_boosts_IsActive ON business_boosts (IsActive);
CREATE INDEX IF NOT EXISTS IX_business_priority_history_BusinessId ON business_priority_history (BusinessId);
CREATE INDEX IF NOT EXISTS IX_business_priority_history_ChangedById ON business_priority_history (ChangedById);
CREATE INDEX IF NOT EXISTS IX_businesses_Priority_IsVerified ON businesses (Priority, IsVerified);
CREATE INDEX IF NOT EXISTS IX_business_reviews_BusinessId_Status_Rating ON business_reviews (BusinessId, Status, Rating);

ALTER TABLE businesses
    DROP INDEX IX_Businesses_PlaceId;