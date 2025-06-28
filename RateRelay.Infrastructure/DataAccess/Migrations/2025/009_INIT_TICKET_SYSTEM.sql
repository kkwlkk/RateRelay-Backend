CREATE TABLE tickets
(
    Id              BIGINT        NOT NULL AUTO_INCREMENT,
    DateCreatedUtc  DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    DateModifiedUtc DATETIME(6)   NULL,
    DateDeletedUtc  DATETIME(6)   NULL,
    Status          TINYINT       NOT NULL DEFAULT 0,
    Type            TINYINT       NOT NULL,
    ReporterId      BIGINT        NOT NULL,
    AssignedToId    BIGINT        NULL,
    Title           VARCHAR(64)   NOT NULL DEFAULT '',
    Description     VARCHAR(2048) NOT NULL DEFAULT '',
    InternalNotes   VARCHAR(4096) NOT NULL DEFAULT '',
    DateStartedUtc  DATETIME(6)   NULL,
    DateResolvedUtc DATETIME(6)   NULL,
    DateClosedUtc   DATETIME(6)   NULL,
    LastActivityUtc DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),

    PRIMARY KEY (Id),

    INDEX IX_Tickets_Status (Status),
    INDEX IX_Tickets_Type (Type),
    INDEX IX_Tickets_ReporterId (ReporterId),
    INDEX IX_Tickets_AssignedToId (AssignedToId),
    INDEX IX_Tickets_DateDeletedUtc (DateDeletedUtc),
    INDEX IX_Tickets_Status_AssignedToId (Status, AssignedToId),
    INDEX IX_Tickets_Status_LastActivityUtc (Status, LastActivityUtc),

    CONSTRAINT FK_Tickets_Reporter
        FOREIGN KEY (ReporterId) REFERENCES accounts (Id)
            ON DELETE RESTRICT,
    CONSTRAINT FK_Tickets_AssignedTo
        FOREIGN KEY (AssignedToId) REFERENCES accounts (Id)
            ON DELETE SET NULL
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

CREATE TABLE ticket_comments
(
    Id                BIGINT        NOT NULL AUTO_INCREMENT,
    DateCreatedUtc    DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    DateModifiedUtc   DATETIME(6)   NULL,
    DateDeletedUtc    DATETIME(6)   NULL,
    TicketId          BIGINT        NOT NULL,
    AuthorId          BIGINT        NOT NULL,
    Content           VARCHAR(2048) NOT NULL DEFAULT '',
    IsInternal        BOOLEAN       NOT NULL DEFAULT FALSE,
    IsSystemGenerated BOOLEAN       NOT NULL DEFAULT FALSE,

    PRIMARY KEY (Id),

    INDEX IX_TicketComments_TicketId (TicketId),
    INDEX IX_TicketComments_AuthorId (AuthorId),
    INDEX IX_TicketComments_DateDeletedUtc (DateDeletedUtc),
    INDEX IX_TicketComments_TicketId_AuthorId (TicketId, AuthorId),

    CONSTRAINT FK_TicketComments_Ticket
        FOREIGN KEY (TicketId) REFERENCES tickets (Id)
            ON DELETE CASCADE,
    CONSTRAINT FK_TicketComments_Author
        FOREIGN KEY (AuthorId) REFERENCES accounts (Id)
            ON DELETE RESTRICT
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

CREATE TABLE ticket_status_history
(
    Id              BIGINT       NOT NULL AUTO_INCREMENT,
    DateCreatedUtc  DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    DateModifiedUtc DATETIME(6)  NULL,
    DateDeletedUtc  DATETIME(6)  NULL,
    TicketId        BIGINT       NOT NULL,
    FromStatus      TINYINT      NULL,
    ToStatus        TINYINT      NOT NULL,
    ChangedById     BIGINT       NOT NULL,
    ChangedReason   VARCHAR(256) NULL,

    PRIMARY KEY (Id),

    INDEX IX_TicketStatusHistory_TicketId (TicketId),
    INDEX IX_TicketStatusHistory_FromStatus (FromStatus),
    INDEX IX_TicketStatusHistory_ToStatus (ToStatus),
    INDEX IX_TicketStatusHistory_ChangedById (ChangedById),
    INDEX IX_TicketStatusHistory_DateDeletedUtc (DateDeletedUtc),
    INDEX IX_TicketStatusHistory_TicketId_DateCreatedUtc (TicketId, DateCreatedUtc),

    CONSTRAINT FK_TicketStatusHistory_Ticket
        FOREIGN KEY (TicketId) REFERENCES tickets (Id)
            ON DELETE CASCADE,
    CONSTRAINT FK_TicketStatusHistory_ChangedBy
        FOREIGN KEY (ChangedById) REFERENCES accounts (Id)
            ON DELETE RESTRICT
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;