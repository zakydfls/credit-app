START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261105034426_AddUsers') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        username character varying(64) NOT NULL,
        normalized_username character varying(64) NOT NULL,
        password_hash text NOT NULL,
        CONSTRAINT "PK_users" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261105034426_AddUsers') THEN
    CREATE UNIQUE INDEX ux_users_normalized_username ON users (normalized_username);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261105034426_AddUsers') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261105034426_AddUsers', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261105054159_AddCreditOwner') THEN
    ALTER TABLE pengajuan_kredit ADD user_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261105054159_AddCreditOwner') THEN
    CREATE INDEX ix_pengajuan_kredit_user_id ON pengajuan_kredit (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261105054159_AddCreditOwner') THEN
    ALTER TABLE pengajuan_kredit ADD CONSTRAINT "FK_pengajuan_kredit_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261105054159_AddCreditOwner') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261105054159_AddCreditOwner', '8.0.11');
    END IF;
END $EF$;
COMMIT;

