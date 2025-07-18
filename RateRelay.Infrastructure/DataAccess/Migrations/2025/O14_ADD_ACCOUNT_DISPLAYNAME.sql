ALTER TABLE accounts
RENAME COLUMN Username TO GoogleUsername;

ALTER TABLE accounts
ADD COLUMN DisplayName VARCHAR(64) NOT NULL AFTER GoogleUsername;

UPDATE accounts
SET DisplayName = GoogleUsername;