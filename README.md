# Gym Progress

Personlig, self-hosted träningslogg. Angular + ASP.NET Core + PostgreSQL. Tänkt att köras hemma och nås via Tailscale.

Data är sanningen. AI-coachen är ett tillägg, inte källan till passen.

## Förutsättningar

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/)
- [Docker](https://docs.docker.com/get-docker/) (till Postgres)
- Valfritt: [Tailscale](https://tailscale.com/) för att nå appen från telefonen
- Valfritt: [LM Studio](https://lmstudio.ai/) om du vill ha gymcoachen

## Hemligheter

All privat konfiguration ligger i `.env`. Den filen är git-ignorerad.

```bash
cp .env.example .env
```

Fyll i minst:

| Variabel | Vad den gör |
| --- | --- |
| `POSTGRES_PASSWORD` | Lösenord till Postgres. Samma värde ska finnas i connection string. |
| `ConnectionStrings__DefaultConnection` | Lokal API → Postgres (port **5433** i utveckling). |
| `JWT_KEY` | Signerar inloggningstokens. Minst 32 tecken. Byt till en egen slumpad sträng. |
| `CORS_ORIGINS` | Komma-separerade webbadresser som får anropa API:t. |
| `AI_BASE_URL` | LM Studio, standard `http://localhost:1234`. |

Lägg din Tailscale-adress i `CORS_ORIGINS`, till exempel:

```env
CORS_ORIGINS=http://localhost:4200,http://127.0.0.1:4200,http://DIN-DATOR.tailnet.ts.net:4200
```

`tailscale status` visar DNS-namnet. Angular tillåter redan alla `*.ts.net`-hostar, så frontenden behöver ingen extra konfig.

## Utveckling

Postgres exponeras på port **5433** så den inte krockar med andra lokala databaser.

```bash
docker compose -f docker-compose.dev.yml up -d
dotnet run --project src/GymProgress.Api
cd frontend && npm start
```

Öppna [http://localhost:4200](http://localhost:4200).

Från en telefon på samma Tailscale-nät:

```text
http://DIN-DATOR.tailnet.ts.net:4200
```

API:t lyssnar på `0.0.0.0:5080`. Frontenden proxar `/api` dit.

Första gången: skapa konto på `/register`. Första kontot tar över eventuell befintlig träningshistorik på servern. Därefter har varje användare sin egen logg.

## Hemmaserver (Docker)

`.env` måste finnas. `JWT_KEY` och `POSTGRES_PASSWORD` saknar fallback i compose.

```bash
cp .env.example .env
# fyll i JWT_KEY, POSTGRES_PASSWORD och CORS_ORIGINS
docker compose up -d --build
```

API: `http://DIN-DATOR.tailnet.ts.net:5080`

Angular körs fortfarande separat under utveckling (`npm start`).

## Coach (LM Studio)

API:t pratar med LM Studio. Angular gör det inte.

Tom `AI_MODEL` använder den modell som är laddad i LM Studio. Systemprompten för gyminstruktören ligger i backend (`src/GymProgress.Application/Coach/GymCoachSystemPrompt.txt`).

Manuell röktest mot en körande LM Studio: ta bort `Skip` på `LmStudioSmokeTests` och kör:

```bash
dotnet test --filter Recommend_against_running_lm_studio
```

## Tester

```bash
dotnet test
cd frontend && npx ng test --watch=false --browsers=ChromeHeadless
```

## Vad som inte ska committas

- `.env`
- lösenord, JWT-nycklar, Tailscale-värdnamn
- `appsettings.*.local.json`

Mallen `.env.example` är okej att pusha. Den innehåller bara placeholders.
