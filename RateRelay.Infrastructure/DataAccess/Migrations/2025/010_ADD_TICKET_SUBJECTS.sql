ALTER TABLE tickets
    ADD COLUMN SubjectBusinessId BIGINT NULL,
    ADD COLUMN SubjectReviewId   BIGINT NULL;

ALTER TABLE tickets
    ADD INDEX IX_Tickets_SubjectBusinessId (SubjectBusinessId),
    ADD INDEX IX_Tickets_SubjectReviewId (SubjectReviewId);

ALTER TABLE tickets
    ADD CONSTRAINT FK_Tickets_SubjectBusiness
        FOREIGN KEY (SubjectBusinessId) REFERENCES businesses (Id)
            ON DELETE SET NULL,
    ADD CONSTRAINT FK_Tickets_SubjectReview
        FOREIGN KEY (SubjectReviewId) REFERENCES business_reviews (Id)
            ON DELETE SET NULL;