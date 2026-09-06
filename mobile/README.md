# Gym Progress — Flutter

Mobilklient mot samma ASP.NET-API som webbappen. Svenskt gränssnitt, cream/grönt tema, animationer och native-känsla.

Webben i `frontend/` är fortfarande produktion. Den här mappen är ett separat Flutter-projekt (iOS, Android, web).

## Förutsättningar

- [Flutter](https://docs.flutter.dev/get-started/install) (SDK 3.14+)
- API:t igång på port **5080** (`dotnet run` eller Docker)

## Kör

Från den här mappen:

```bash
cd mobile
flutter pub get
flutter run --dart-define=API_BASE_URL=http://127.0.0.1:5080
```

På inloggningsskärmen kan du också byta API-adress.

| Var du kör appen | API-adress |
| --- | --- |
| iOS-simulator / macOS | `http://127.0.0.1:5080` |
| Android-emulator | `http://10.0.2.2:5080` |
| Riktig telefon via Tailscale | `http://DIN-DATOR.tailnet.ts.net:5080` |

Flutter pratar direkt med API:t (inte Angular-proxyn). CORS spelar ingen roll för native, men HTTP utan TLS är tillåtet i debug så du kan nå hemmaservern.

Samma konto som på webben. Första kontot tar över eventuell seed-historik.

## Vad som finns

Inloggning, hem, pass, set-loggning med vila och PR-feedback, historik, mallar, rekord, progressdiagram, profil och kroppsmått. Coachen går via backend precis som på webben.
