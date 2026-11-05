# Local containers

## Start API and PostgreSQL 16.3

Create the local secret files from the examples:

```sh
mkdir -p secrets
cp -n secrets/postgres_password.txt.example secrets/postgres_password.txt
cp -n secrets/connection_string.txt.example secrets/connection_string.txt
cp -n secrets/jwt_signing_key.txt.example secrets/jwt_signing_key.txt
```

Set the same PostgreSQL password in `postgres_password.txt` and in the `Password=` part of `connection_string.txt`. Replace the example JWT key before using the API.

Start the stack:

```sh
docker compose up --build -d
docker compose logs -f api
```

Swagger is available at <http://localhost:8080/swagger>. PostgreSQL is available from the Mac at `localhost:5434`; containers connect to it at `db:5432`.

The schema is initialized when PostgreSQL starts with an empty `postgres_data` volume. The named volume keeps database contents across `docker compose down` and restarts.

## Run the API directly with .NET User Secrets

For local development outside Compose, store credentials in .NET User Secrets instead of a `.env` file:

```sh
dotnet user-secrets set "ConnectionStrings:CreditDatabase" "Host=localhost;Port=5432;Database=credit_application;Username=postgres;Password=YOUR_LOCAL_DB_PASSWORD" --project src/CreditApplication.Api
dotnet user-secrets set "Jwt:SigningKey" "YOUR_RANDOM_SECRET_AT_LEAST_32_CHARACTERS" --project src/CreditApplication.Api
dotnet ef database update --project src/CreditApplication.Infrastructure --startup-project src/CreditApplication.Api
dotnet run --project src/CreditApplication.Api
```

JWT issuer and audience are defined in `appsettings.json`. Compose secrets are mounted under `/run/secrets` and loaded by the .NET key-per-file configuration provider. In production, provide secret values through the platform's secret manager.

In Development, a demo user is seeded at API startup after migration: username `skyworx`, password `demo12345`. The seeder does not change an existing user's password. Set `SeedDemoUser` to `false` to disable it; it never runs outside Development.

You can also register a user through `POST /api/auth/register`, then log in through `POST /api/auth/login`. Usernames contain 3–64 ASCII letters, digits, dots, underscores or hyphens; passwords contain 8–128 characters. Registration returns 409 when the username already exists (case-insensitive). Compose applies the checked-in migration script to an existing volume before starting the API.

Credit applications are scoped to the user ID in the JWT. Existing JWTs issued before owner support must be replaced by logging in again. Legacy credit rows without an owner remain in the database but are not accessible through the API until assigned to a user.
