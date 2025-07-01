ALTER TABLE ticket_comments
    ADD COLUMN DateEditedUtc DATETIME(6) NULL AFTER DateModifiedUtc;