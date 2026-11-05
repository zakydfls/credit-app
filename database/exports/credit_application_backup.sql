--
-- PostgreSQL database dump
--

\restrict 2IC99wZa9vVOJz7VnVFjtIrzPpcV3J65naZMXPzGe69Duou7pCaQN3IpcL0KEZb
    
SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Name: pengajuan_kredit; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pengajuan_kredit (
    id uuid NOT NULL,
    plafon numeric(18,2) NOT NULL,
    bunga numeric(5,2) NOT NULL,
    tenor integer NOT NULL,
    angsuran numeric(18,2) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    user_id uuid,
    CONSTRAINT ck_pengajuan_kredit_angsuran_positive CHECK ((angsuran > (0)::numeric)),
    CONSTRAINT ck_pengajuan_kredit_bunga_range CHECK (((bunga > (0)::numeric) AND (bunga <= (100)::numeric))),
    CONSTRAINT ck_pengajuan_kredit_plafon_positive CHECK ((plafon > (0)::numeric)),
    CONSTRAINT ck_pengajuan_kredit_tenor_positive CHECK ((tenor > 0))
);


--
-- Name: users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.users (
    id uuid NOT NULL,
    username character varying(64) NOT NULL,
    normalized_username character varying(64) NOT NULL,
    password_hash text NOT NULL
);


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20261104115511_InitialCreate	8.0.11
20261105034426_AddUsers	8.0.11
20261105054159_AddCreditOwner	8.0.11
\.


--
-- Data for Name: pengajuan_kredit; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.pengajuan_kredit (id, plafon, bunga, tenor, angsuran, created_at, updated_at, user_id) FROM stdin;
02c67499-758b-4c0b-b39f-d91e509bbb80	100000000.00	12.00	60	2224444.77	2026-11-05 15:48:39.236368+07	2026-11-05 15:48:39.236506+07	10057e76-13b7-45a7-a82a-110c440cd7b7
011d2376-e7c7-4682-9103-73b986e8b43e	200000000.00	12.00	30	7749622.64	2026-11-05 21:02:48.115219+07	2026-11-05 21:02:48.115501+07	731bd2a5-04e6-4f4a-9c55-8396b7530f30
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.users (id, username, normalized_username, password_hash) FROM stdin;
10057e76-13b7-45a7-a82a-110c440cd7b7	skyworx	SKYWORX	AQAAAAIAAYagAAAAEJtfwTFxFsI+ahGAFS4RTRp3WG7A1FGxmCrPbmkxk1LI80rD+sRYmKMNmfYq8nsQhw==
731bd2a5-04e6-4f4a-9c55-8396b7530f30	zaky	ZAKY	AQAAAAIAAYagAAAAENHaFlTnumlXYi5Nn1f5e3k8oK4VCpp1ogk9HFTwQcG+bzmQClHOKdN0/2gT/o3J7A==
\.


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: pengajuan_kredit PK_pengajuan_kredit; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pengajuan_kredit
    ADD CONSTRAINT "PK_pengajuan_kredit" PRIMARY KEY (id);


--
-- Name: users PK_users; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT "PK_users" PRIMARY KEY (id);


--
-- Name: ix_pengajuan_kredit_plafon; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pengajuan_kredit_plafon ON public.pengajuan_kredit USING btree (plafon);


--
-- Name: ix_pengajuan_kredit_tenor; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pengajuan_kredit_tenor ON public.pengajuan_kredit USING btree (tenor);


--
-- Name: ix_pengajuan_kredit_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pengajuan_kredit_user_id ON public.pengajuan_kredit USING btree (user_id);


--
-- Name: ux_users_normalized_username; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ux_users_normalized_username ON public.users USING btree (normalized_username);


--
-- Name: pengajuan_kredit FK_pengajuan_kredit_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pengajuan_kredit
    ADD CONSTRAINT "FK_pengajuan_kredit_users_user_id" FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

\unrestrict 2IC99wZa9vVOJz7VnVFjtIrzPpcV3J65naZMXPzGe69Duou7pCaQN3IpcL0KEZb

