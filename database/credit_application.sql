-- PostgreSQL schema export for the CreditApplication API.
-- Restore into a database created with: CREATE DATABASE credit_application;

CREATE TABLE IF NOT EXISTS public.pengajuan_kredit (
    id uuid PRIMARY KEY,
    plafon numeric(18, 2) NOT NULL,
    bunga numeric(5, 2) NOT NULL,
    tenor integer NOT NULL,
    angsuran numeric(18, 2) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT ck_pengajuan_kredit_plafon_positive CHECK (plafon > 0),
    CONSTRAINT ck_pengajuan_kredit_bunga_range CHECK (bunga > 0 AND bunga <= 100),
    CONSTRAINT ck_pengajuan_kredit_tenor_positive CHECK (tenor > 0),
    CONSTRAINT ck_pengajuan_kredit_angsuran_positive CHECK (angsuran > 0)
);

CREATE INDEX IF NOT EXISTS ix_pengajuan_kredit_plafon
    ON public.pengajuan_kredit (plafon);

CREATE INDEX IF NOT EXISTS ix_pengajuan_kredit_tenor
    ON public.pengajuan_kredit (tenor);

CREATE TABLE IF NOT EXISTS public.users (
    id uuid PRIMARY KEY,
    username character varying(64) NOT NULL,
    normalized_username character varying(64) NOT NULL,
    password_hash text NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_users_normalized_username
    ON public.users (normalized_username);

ALTER TABLE public.pengajuan_kredit ADD COLUMN IF NOT EXISTS user_id uuid;
CREATE INDEX IF NOT EXISTS ix_pengajuan_kredit_user_id
    ON public.pengajuan_kredit (user_id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.pengajuan_kredit'::regclass
          AND conname = 'FK_pengajuan_kredit_users_user_id'
    ) THEN
        ALTER TABLE public.pengajuan_kredit
            ADD CONSTRAINT "FK_pengajuan_kredit_users_user_id"
            FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE RESTRICT;
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) PRIMARY KEY,
    "ProductVersion" character varying(32) NOT NULL
);

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261104115511_InitialCreate', '8.0.11')
ON CONFLICT ("MigrationId") DO NOTHING;

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261105034426_AddUsers', '8.0.11')
ON CONFLICT ("MigrationId") DO NOTHING;

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261105054159_AddCreditOwner', '8.0.11')
ON CONFLICT ("MigrationId") DO NOTHING;

-- One row with the longest tenor; plafon breaks ties between those rows.
SELECT id, plafon, bunga, tenor, angsuran, created_at, updated_at
FROM public.pengajuan_kredit
ORDER BY tenor DESC, plafon DESC, id
LIMIT 1;

-- Average annual interest rate across all applications.
SELECT AVG(bunga) AS rata_rata_bunga
FROM public.pengajuan_kredit;
