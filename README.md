# Gym Progress

Personlig, self-hosted träningslogg. Angular + ASP.NET Core + PostgreSQL. Tänkt att köras hemma och nås via Tailscale.

Data är sanningen. AI-coachen är ett tillägg, inte källan till passen.

## Förutsättningar

- [Docker](https://docs.docker.com/get-docker/) (hela stacken, eller bara Postgres i utveckling)
- För lokal utveckling utan Docker-frontend/API: [.NET 10 SDK](https://dotnet.microsoft.com/download) och [Node.js 22](https://nodejs.org/)
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
| `WEB_PORT` | Host-port för Docker-webben (nginx), standard **80**. |
| `AI_BASE_URL` | LM Studio, standard `http://localhost:1234`. |

Lägg din Tailscale-adress i `CORS_ORIGINS`. Med Docker på port 80 behövs ingen port i URL:en:

```env
CORS_ORIGINS=http://localhost,http://127.0.0.1,http://localhost:4200,http://DIN-DATOR.tailnet.ts.net
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

Hela appen körs i Compose: Postgres, API och Angular bakom nginx. `.env` måste finnas. `JWT_KEY` och `POSTGRES_PASSWORD` saknar fallback i compose.

```bash
cp .env.example .env
# fyll i JWT_KEY, POSTGRES_PASSWORD och CORS_ORIGINS (Tailscale-adressen utan port om WEB_PORT=80)
docker compose up -d --build
```

Webbappen: [http://localhost](http://localhost) (eller `http://DIN-DATOR.tailnet.ts.net`). nginx proxar `/api` till API:t, så du behöver inte öppna port 5080 från telefonen.

API direkt (valfritt): `http://localhost:5080/health`

Stoppa med `docker compose down`. Volymen `postgres_data` behåller databasen.

Lokal kodning mot bara Postgres är oförändrad: `docker-compose.dev.yml` + `dotnet run` + `npm start`.

## GitHub Actions

Varje push och PR mot `Main` kör tester och startar sedan samma Docker-stack på GitHub-runnern. Jobbet väntar tills containrarna är healthy, röktestar `/`, `/health` och `/api`, och stänger ner allt.

GitHub Actions är tillfällig — den bevisar att stacken bygger och startar. Den ersätter inte hemmaservern.

## Auto-uppdatera Mac mini från Main

På servern kollar ett skript GitHub var tredje minut. Finns det ny kod på `Main` **och** CI är grön hämtas den och Docker byggs om. `.env` ligger utanför git och skrivs inte över.

```bash
chmod +x scripts/update-from-main.sh scripts/install-update-agent.sh
./scripts/install-update-agent.sh
```

Logg: `~/Library/Logs/gymprogress-update.log`

Skriptet hoppar över uppdateringen om du står på en annan gren eller har osparade filer, så utvecklingsjobb i samma mapp inte skrivs över. Kodar du ofta lokalt: gör det på en feature-gren, och låt `Main` vara den som servern följer.

Manuell körning:

```bash
./scripts/update-from-main.sh
```

Avinstallera agenten:

```bash
launchctl bootout gui/$(id -u)/se.gymprogress.update
rm -f ~/Library/LaunchAgents/se.gymprogress.update.plist
```

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
