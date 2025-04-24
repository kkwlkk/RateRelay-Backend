ALTER TABLE businesses
    ADD COLUMN Priority TINYINT UNSIGNED NOT NULL DEFAULT 0 AFTER IsVerified;

CREATE TABLE business_reviews
(
    Id              bigint   NOT NULL AUTO_INCREMENT PRIMARY KEY,
    BusinessId      bigint   NOT NULL,
    ReviewerId      bigint   NOT NULL,
    Status          tinyint  NOT NULL,
    DateAcceptedUtc datetime NULL,
    DateRejectedUtc datetime NULL,
    DateCreatedUtc  datetime NOT NULL,
    DateModifiedUtc datetime NULL,
    DateDeletedUtc  datetime NULL,
    CONSTRAINT FK_business_reviews_businesses_BusinessId FOREIGN KEY (BusinessId) REFERENCES businesses (Id),
    CONSTRAINT FK_business_reviews_accounts_AccountId FOREIGN KEY (ReviewerId) REFERENCES accounts (Id)
);

CREATE INDEX IX_business_reviews_BusinessId ON business_reviews (BusinessId);
CREATE INDEX IX_business_reviews_ReviewerId ON business_reviews (ReviewerId);
CREATE INDEX IX_business_reviews_Status ON business_reviews (Status);