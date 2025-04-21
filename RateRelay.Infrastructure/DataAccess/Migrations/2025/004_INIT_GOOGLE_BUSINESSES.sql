CREATE TABLE businesses
(
    Id              BIGINT PRIMARY KEY AUTO_INCREMENT,
    PlaceId         VARCHAR(255) NOT NULL,
    Cid             VARCHAR(255) NOT NULL,
    BusinessName    VARCHAR(255) NOT NULL,
    OwnerAccountId  BIGINT       NOT NULL,
    IsVerified      BOOLEAN      NOT NULL DEFAULT 0,
    DateCreatedUtc  DATETIME     NOT NULL,
    DateModifiedUtc DATETIME     NULL,
    DateDeletedUtc  DATETIME     NULL,
    CONSTRAINT FK_Businesses_Accounts FOREIGN KEY (OwnerAccountId) REFERENCES accounts (Id),
    UNIQUE KEY IX_Businesses_PlaceId (PlaceId)
);

CREATE TABLE business_verifications
(
    Id                        BIGINT PRIMARY KEY AUTO_INCREMENT,
    BusinessId                BIGINT   NOT NULL,
    VerificationStartedUtc    DATETIME NOT NULL,
    VerificationCompletedUtc  DATETIME NULL,
    VerificationDay           TINYINT  NOT NULL, -- 0 = Sunday, 6 = Saturday
    verification_opening_time TIME     NOT NULL,
    verification_closing_time TIME     NOT NULL,
    VerificationAttempts      INT      NOT NULL DEFAULT 0,
    DateCreatedUtc            DATETIME NOT NULL,
    DateModifiedUtc           DATETIME NULL,
    DateDeletedUtc            DATETIME NULL,
    CONSTRAINT FK_BusinessVerifications_Businesses FOREIGN KEY (BusinessId) REFERENCES businesses (Id) ON DELETE CASCADE,
    INDEX IX_BusinessVerifications_BusinessId (BusinessId)
);