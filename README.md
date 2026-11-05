# Credit Application API

A REST API for managing credit applications and calculating monthly installments. Users register and sign in to obtain a JWT; new credit applications are linked to the authenticated owner.

This README explains how to run the project locally or with Docker, use the API, manage migrations, and find the implementation for questions 1–5 of the technical assessment.

## Technology and structure

- .NET 8, ASP.NET Core Web API, Entity Framework Core, and PostgreSQL 16.3.
- JWT Bearer, ASP.NET Core Identity PasswordHasher, Serilog, Swagger/OpenAPI, and xUnit.
- Docker and Docker Compose for running the API and database.

```text
src/
  CreditApplication.Api/             HTTP controllers, contracts, JWT validation
  CreditApplication.Application/     Use cases, commands, DTOs, and interfaces
  CreditApplication.Domain/          Entities and installment calculation
  CreditApplication.Infrastructure/  EF Core, repositories, password hashing, JWT
tests/
  CreditApplication.UnitTests/         Domain and application unit tests
  CreditApplication.IntegrationTests/  HTTP API tests
database/
  credit_application.sql              Schema and queries for question 1
  add_users.sql                       Idempotent migration script for Compose
secrets/
  *.example                            Example secret-file formats for Compose
```

The main flow is `Controller → Application service → interface → Infrastructure implementation → PostgreSQL`. Application results are mapped to API contracts and wrapped in a standard response.

## Prerequisites

Use Docker and Docker Compose to run the complete stack, or .NET SDK 8 and PostgreSQL 16.3 to run the API directly on your machine. PostgreSQL clients such as `psql` and `createdb` are useful for managing a local database.

Check the tools relevant to your chosen approach:

```sh
dotnet --version
docker compose version
psql --version
```

`psql --version` reports the **client** version. Run `SELECT version();` against the database to check the server version.

## Run with Docker Compose

From the repository root, create local secret files from the examples. The `-n` option avoids overwriting files that already exist:

```sh
cp -n secrets/postgres_password.txt.example secrets/postgres_password.txt
cp -n secrets/connection_string.txt.example secrets/connection_string.txt
cp -n secrets/jwt_signing_key.txt.example secrets/jwt_signing_key.txt
```

Edit the resulting `.txt` files:

1. Put the local PostgreSQL password in `postgres_password.txt`.
2. Set `connection_string.txt` to a connection string for the Compose network, for example: `Host=db;Port=5432;Database=credit_application;Username=postgres;Password=THE_SAME_PASSWORD`.
3. Put a random JWT signing secret of at least **32 bytes** in `jwt_signing_key.txt`.

The password in the two database files must match. Local secret files are ignored by Git and excluded from the Docker build context. Example values are placeholders for development.

```sh
docker compose up --build -d
docker compose ps -a
docker compose logs -f api
```

Compose starts `db` (PostgreSQL 16.3), `migrate`, and `api`. An `Exited (0)` status for `migrate` after it finishes is expected. Open **http://localhost:8080/swagger**. PostgreSQL is accessible from the host at `localhost:5434`; the API container connects through `db:5432`.

The initial schema is created when the database volume is empty. `docker compose down` stops and removes containers while preserving the named volume. Changing a password in a secret file after the volume has been initialized does not automatically change the existing database user's password.

## Run the API locally

### 1. Prepare the database

Start PostgreSQL 16.3 and create the database **if it does not exist**:

```sh
createdb -h localhost -p 5432 -U postgres credit_application
```

To use only the database service from Compose, run `docker compose up -d db` and use host port `5434` in the local API connection string.

### 2. Configure the application

ASP.NET Core reads non-secret settings from `appsettings.json` and the matching `appsettings.{Environment}.json` file. JWT issuer and audience defaults are in `appsettings.json`. Use .NET User Secrets for local development credentials:

```sh
dotnet user-secrets set "ConnectionStrings:CreditDatabase" \
  "Host=localhost;Port=5432;Database=credit_application;Username=postgres;Password=REPLACE_LOCAL_PASSWORD" \
  --project src/CreditApplication.Api
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -hex 32)" \
  --project src/CreditApplication.Api
```

Adjust the port, username, and password for your database server. Commands containing a password can be stored in shell history. User Secrets are stored outside the project for development and are not encrypted; use a secret manager for production.

Configuration providers override earlier values for the same key. If `Jwt:Issuer` or `Jwt:Audience` is also present in User Secrets, an environment variable, or another higher-priority provider, that value overrides the default. This project adds the optional key-per-file provider for `/run/secrets` after the default providers.

### 3. Restore, migrate, and run

```sh
dotnet tool restore
dotnet restore
dotnet ef database update \
  --project src/CreditApplication.Infrastructure \
  --startup-project src/CreditApplication.Api
dotnet run --project src/CreditApplication.Api --launch-profile http
```

The `http` profile sets the environment to `Development` and listens on port `5224`. Open **http://localhost:5224/swagger**. Ensure the User Secrets connection string points to the database you intend to migrate. Swagger is enabled in Development.

### Select an environment

`appsettings.json` contains shared, non-secret defaults. When the selected environment is `Development`, `Staging`, or `Production`, .NET also loads the corresponding `appsettings.{Environment}.json` if it exists. Environment-specific files may be committed if they contain no secrets. This project currently includes `appsettings.Development.json`; it does not include staging or production files.

For example, to select Staging without the launch profile overriding the environment:

```sh
ASPNETCORE_ENVIRONMENT=Staging \
  dotnet run --project src/CreditApplication.Api --no-launch-profile -- \
  --urls http://localhost:5224
```

Provide the required connection string and JWT signing key through environment variables or a secret manager for that environment; User Secrets are loaded automatically only in Development. Swagger is not enabled for Staging by the current configuration. Compose currently sets `ASPNETCORE_ENVIRONMENT` to `Development` in `docker-compose.yml`.

## Authenticate and try the API

In the `Development` environment, the application seeds a demo user when it starts, after the database has been migrated:

```text
Username: skyworx
Password: demo12345
```

The seeder is enabled by `SeedDemoUser: true` in `appsettings.Development.json`. It runs only in Development, and can be disabled by setting that key to `false`. If `skyworx` already exists, its password is not changed; the existing password must be used. The fixed credentials are for local demonstration only.

Passwords are hashed with ASP.NET Core Identity `PasswordHasher`. JWTs use HS256 and expire after 15 minutes.

1. Use `POST /api/auth/login` with the Development demo account, or create a different account through `POST /api/auth/register`:
   ```json
    {
      "username": "zaky",
      "password": "PasswordLokal123!"
    }
   ```
2. Send the credentials of your chosen account to `POST /api/auth/login` and copy `data.accessToken`.
3. Select **Authorize** and enter the token without the `Bearer` prefix. Swagger sends `Authorization: Bearer <token>`.
4. Try a protected endpoint. Sign in again after the token expires.

Usernames contain 3–64 ASCII letters, digits, periods, underscores, or hyphens; passwords contain 8–128 characters. Usernames are unique without regard to letter case. The example password is for local use only.

| Method | Endpoint | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | No | Register a user |
| POST | `/api/auth/login` | No | Obtain a JWT |
| POST | `/api/credit-applications` | JWT | Create a credit application |
| GET | `/api/credit-applications?page=1&pageSize=20` | JWT | List the user's applications |
| GET | `/api/credit-applications/{id}` | JWT | Get the user's application |
| PUT | `/api/credit-applications/{id}` | JWT | Update the user's application |
| DELETE | `/api/credit-applications/{id}` | JWT | Delete the user's application |
| POST | `/api/installments/calculate` | JWT | Calculate without saving |

`page` starts at 1; `pageSize` defaults to 20 and must be 1–100. The list response contains `items`, `page`, `pageSize`, `totalCount`, and `totalPages`.

Example body for create, update, and calculation:

```json
{
  "plafon": 100000000,
  "bunga": 12,
  "tenor": 60
}
```

`bunga` is an **annual percentage** and `tenor` is in months. The owner `userId` is taken from the JWT, not the request body. Update recalculates the installment from the new values.

## Response format

Successful calculation example:

```json
{
  "data": { "angsuran": 2224444.77 },
  "message": "Angsuran bulanan berhasil dihitung.",
  "success": true,
  "requestId": "EXAMPLE_REQUEST_ID"
}
```

Validation error example:

```json
{
  "message": "One or more validation errors occurred.",
  "status": 400,
  "success": false,
  "errors": {
    "Plafon": ["Plafon harus lebih besar dari 0 dan maksimal 16 digit sebelum desimal."]
  },
  "requestId": "EXAMPLE_REQUEST_ID"
}
```

Messages in these examples remain in Indonesian because they match the current API responses. The API handles `400` for invalid input, `401` for invalid credentials/token, `404` for an application that is missing or owned by another user, `409` for a duplicate username, and `500` for unhandled exceptions. Default responses for unmatched routes are not guaranteed to use this envelope.

## Assessment questions 1–5

### Question 1 — Database and queries

The primary `pengajuan_kredit` table has the requested columns: UUID `id`; `plafon` and `angsuran` as `numeric(18,2)`; `bunga` as `numeric(5,2)`; integer `tenor`; and `created_at` and `updated_at` as `timestamp with time zone`. The `user_id` column and `users` table support authentication and ownership. `user_id` is nullable to preserve legacy data; new applications through the API always receive their owner from the JWT.

Separate indexes exist on `plafon` and `tenor`; the `user_id` index supports owner filtering. Indexes should be evaluated against actual queries and execution plans.

The requested queries are in `database/credit_application.sql`:

```sql
-- Longest tenor first; highest plafon breaks a tenor tie.
SELECT id, plafon, bunga, tenor, angsuran, created_at, updated_at
FROM public.pengajuan_kredit
ORDER BY tenor DESC, plafon DESC, id
LIMIT 1;

SELECT AVG(bunga) AS rata_rata_bunga
FROM public.pengajuan_kredit;
```

The first query prioritizes tenor, so its plafon is not necessarily the global maximum. These SQL queries cover the entire table; API endpoints remain scoped to the owner.

`database/credit_application.sql` contains the schema and queries; `database/add_users.sql` is the Compose migration script. Neither is a current data export. To create a **SQL backup containing schema and data** from the running Compose database:

```sh
docker compose exec -T db pg_dump -U postgres -d credit_application \
  --no-owner --no-privileges > database/credit_application_backup.sql
```

A backup can contain credit data and password hashes. Files matching `database/*_backup.sql` and files in `database/exports/` are ignored by Git and excluded from the Docker build context; include the backup separately when submitting. Review its contents and share it only through an authorized channel. To restore a plain SQL backup into a newly created, empty database, run `psql -U postgres -d credit_application -v ON_ERROR_STOP=1 -f database/credit_application_backup.sql`.

### Question 2 — CRUD, JWT, and validation

The endpoints are listed above. The JWT carries the user ID, and the repository scopes operations to that owner. `plafon`, `bunga`, and `tenor` must be greater than zero; interest cannot exceed 100 percent. `plafon` and `bunga` allow at most two decimal places. The resulting installment must be positive and fit `numeric(18,2)`.

Request validation uses Data Annotations and `IValidatableObject`. A user ID supplied in the request body is not used to set the owner.

### Question 3 — Installment calculation

The implementation uses an annuity formula with an annual interest rate:

```text
r = annual interest rate / 100 / 12
n = tenor in months
installment = principal × r / (1 − (1 + r)^(-n))
```

The result is rounded to two decimal places using `MidpointRounding.AwayFromZero`. For a principal of Rp100,000,000, annual interest of 12%, and a 60-month tenor, the result is **Rp2,224,444.77 per month**. The calculation endpoint does not persist data; create and update use the same formula.

### Question 4 — Tests

```sh
dotnet test
```

Unit tests cover the calculator and authentication service. Integration tests exercise HTTP requests, register/login, validation, CRUD, JWT, and access restrictions between users. Depending on the scenario, HTTP tests use EF Core InMemory or a fake service; they do not replace full integration testing against a running PostgreSQL server.

### Question 5 — Error handling and logging

Handled error responses contain `message`, `status`, `success`, `errors`, and `requestId`. Invalid input returns `400`; a missing application or one belonging to another user returns `404`. The global exception handler handles uncaught exceptions as `500`.

Serilog records request and response **metadata** through `UseSerilogRequestLogging`: HTTP method, path, response status, and duration. `RequestId` is added as a diagnostic-context property; the default console template may not display it. **Request and response bodies are not logged.** If the assessor expects full bodies, that requirement needs clarification and sensitive data such as passwords and tokens must be redacted.

Logs go to the console. With Compose:

```sh
docker compose logs -f api
```

## Create a new migration

1. Change the entity under `Domain/Entities` and its mapping under `Infrastructure/Persistence/Configurations`. For a new entity, also add a `DbSet` to `CreditApplicationDbContext`.
2. Create and review the migration:
   ```sh
    dotnet ef migrations add ChangeName \
      --project src/CreditApplication.Infrastructure \
      --startup-project src/CreditApplication.Api \
      --output-dir Persistence/Migrations
   ```
3. For the database targeted by local User Secrets, apply it with `dotnet ef database update` using the project/startup arguments shown in the local run section.
4. For the current Compose setup, regenerate the migration script mounted by `migrate`:
   ```sh
    dotnet ef migrations script 20261104115511_InitialCreate --idempotent \
      --project src/CreditApplication.Infrastructure \
      --startup-project src/CreditApplication.Api \
      --output database/add_users.sql
    docker compose run --rm migrate
    docker compose up -d --build api
   ```

That script targets project databases that already have `InitialCreate`, including a new database initialized by `credit_application.sql`. Review the script and plan data changes before applying it. Update the schema export `credit_application.sql` when the schema changes.
