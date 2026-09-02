<div align="center">

# RateRelay Backend

**Platforma wymiany opinii dla wizytówek Google Business Profile.**
Właściciele firm zdobywają opinie, wystawiając opinie innym, w oparciu o system punktów i kolejkę.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MariaDB](https://img.shields.io/badge/MariaDB-10.11-003545?logo=mariadb&logoColor=white)](https://mariadb.org/)
[![Redis](https://img.shields.io/badge/Redis-7.2-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![Hangfire](https://img.shields.io/badge/Hangfire-1.8-blue)](https://www.hangfire.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)

### 🌐 Language / Język

[🇬🇧 English](readme.md) &nbsp;·&nbsp; **🇵🇱 Polski**

</div>

---

## Spis treści

- [Czym jest RateRelay?](#czym-jest-raterelay)
- [Jak to działa](#jak-to-działa)
- [Architektura](#architektura)
- [Stos technologiczny](#stos-technologiczny)
- [Funkcje](#funkcje)
- [Pierwsze uruchomienie](#pierwsze-uruchomienie)
- [Konfiguracja](#konfiguracja)
- [Przegląd API](#przegląd-api)
- [System punktów](#system-punktów)
- [Zadania w tle](#zadania-w-tle)
- [Uprawnienia](#uprawnienia)
- [Struktura projektu](#struktura-projektu)
- [Wdrożenie](#wdrożenie)
- [Znane ograniczenia](#znane-ograniczenia)

---

## Czym jest RateRelay?

RateRelay to backend (REST API) platformy, na której właściciele lokalnych firm wymieniają się
uczciwymi opiniami. Aby otrzymywać opinie o własnej firmie, trzeba najpierw oceniać inne. Każda
zaakceptowana opinia daje punkty, a punkty decydują o tym, czy Twoja firma pozostaje widoczna
w kolejce innych użytkowników.

API to modułowe rozwiązanie w **ASP.NET Core 8** zbudowane zgodnie z Clean Architecture: CQRS
oparte na MediatR, MariaDB jako baza danych, Redis do cache'owania i blokad rozproszonych oraz
Hangfire do cyklicznych zadań w tle.

---

## Jak to działa

```
 1. Logowanie przez Google
        │
        ▼
 2. Onboarding: zgłoszenie własnej firmy (Google Place ID)
        │
        ▼
 3. Wyzwanie weryfikacyjne
    API losuje dzień oraz godziny otwarcia i zamknięcia.
    Właściciel ustawia dokładnie te godziny w wizytówce Google,
    a API odczytuje je ponownie przez Google Places API i potwierdza własność.
        │
        ▼
 4. Kolejka do oceniania
    GET /api/user/reviewable-businesses/next przydziela firmę do oceny
    (blokada w Redisie na 10 minut, aby nikt inny nie dostał tej samej).
        │
        ▼
 5. Wystawienie opinii (ocena + komentarz, opcjonalnie publiczna opinia w Google Maps)
    Punkty zostają zablokowane na saldzie ocenianego właściciela.
        │
        ▼
 6. Właściciel akceptuje lub odrzuca opinię
    Akceptacja  → oceniający otrzymuje punkty
    Odrzucenie  → punkty wracają do właściciela
    Brak reakcji przez 7 dni → automatyczna akceptacja przez nocne zadanie
        │
        ▼
 7. Saldo ≥ 2 punkty → Twoja firma pojawia się w kolejkach innych użytkowników
```

**Priorytet i wyróżnienia (boost).** Każda firma ma bajtowy `Priority`; administrator może
podbić firmę, aby trafiała wyżej w kolejce. Pominięte firmy są ukrywane przed danym
użytkownikiem na 12 godzin, a firma odrzucona trzykrotnie znika z jego kolejki na stałe.

---

## Architektura

Clean Architecture w czterech projektach, z zależnościami skierowanymi wyłącznie do wewnątrz:

```
RateRelay.API             ← Kontrolery, middleware, filtry, atrybuty, Swagger
        │
RateRelay.Application     ← CQRS (komendy/zapytania/handlery MediatR), DTO,
        │                   FluentValidation, profile AutoMappera, zadania Hangfire
RateRelay.Infrastructure  ← EF Core + Pomelo/MySQL, repozytoria, Unit of Work,
        │                   Redis, serwisy, e-mail, migracje dbup, Serilog
RateRelay.Domain          ← Encje, enumy, stałe, interfejsy, wyjątki
```

Zastosowane wzorce:

| Wzorzec | Gdzie |
| --- | --- |
| **CQRS** | `Features/{Obszar}/{Commands,Queries}/…` (jeden katalog na przypadek użycia) |
| **Behaviory MediatR** | `LoggingBehavior`, `ValidationBehavior`, blokada zbanowanych kont |
| **Repository + Unit of Work** | `IUnitOfWorkFactory` → `IRepository<T>` / `IExtendedRepository<T>` |
| **Ujednolicona koperta odpowiedzi** | Każdy endpoint zwraca `ApiResponse<T>` / `PagedApiResponse<T>` |
| **Blokady rozproszone** | `IRedisDistributedLockProvider` chroni przydziały w kolejce |
| **Szyfrowanie pól** | Atrybut `[Encrypted]` + konwerter EF (np. adres e-mail konta) |
| **Maska bitowa uprawnień** | Flagi `ulong` na koncie, sprawdzane przez `[RequirePermission]` |

---

## Stos technologiczny

| Obszar | Technologia |
| --- | --- |
| Środowisko uruchomieniowe | .NET 8 / ASP.NET Core |
| Baza danych | MariaDB 10.11 (EF Core 9 + `Pomelo.EntityFrameworkCore.MySql`) |
| Migracje schematu | **dbup-mysql** ze skryptami SQL osadzonymi w zasobach (`DataAccess/Migrations/2025/*.sql`) |
| Cache i blokady | Redis 7.2 (`StackExchange.Redis`, `DistributedLock.Redis`) |
| Zadania w tle | Hangfire + `Hangfire.Redis.StackExchange` |
| Mediacja / CQRS | MediatR 12 |
| Walidacja | FluentValidation 12 (preview) |
| Mapowanie | AutoMapper 14 |
| Uwierzytelnianie | Google OAuth (`Google.Apis.Auth`) + JWT bearer z tokenami odświeżania |
| E-mail | MailKit + szablony **Fluid** (`.liquid`), minifikacja WebMarkupMin |
| Logowanie | Serilog (konsola + pliki rotowane, własne enrichery) |
| Dokumentacja | Swashbuckle / Swagger UI |
| Hosting | Docker (wieloetapowy `Dockerfile`) + docker-compose |

---

## Funkcje

### Uwierzytelnianie i konta
- Logowanie Google OAuth wymieniane na token JWT (1 h) i token odświeżania (14 dni).
- Rotacja tokenów odświeżania; wygasłe są usuwane co 30 minut.
- Bany kont (`AccountBanEntity`) egzekwowane przez behavior w pipeline MediatR; wygasłe bany
  są zdejmowane automatycznie co 5 minut.
- Flagi konta i preferencje e-mailowe przechowywane jako flagi bitowe.
- Adresy e-mail są szyfrowane w bazie danych.

### Onboarding
Trzyetapowy proces (`BusinessVerification → Welcome → Completed`) wymuszany po stronie serwera
atrybutem `[RequireOnboardingStep]`, więc użytkownik nie pominie żadnego kroku.

### Weryfikacja firmy
Własność potwierdzana bez ręcznej moderacji: API losuje dzień oraz godziny otwarcia i zamknięcia,
właściciel ustawia je w wizytówce Google, a `POST /api/user/business/verification/process`
pobiera ponownie `currentOpeningHours` z Google Places API i porównuje. Wyzwanie wygasa po
7 dniach, liczba prób jest zliczana.

### Kolejka do oceniania
- Sprawiedliwy przydział oparty na blokadach rozproszonych w Redisie (10 minut na firmę).
- Wykluczane są: firma własna, firmy już ocenione oraz firmy odrzucone trzykrotnie; pominięte
  pozostają ukryte przez 12 godzin.
- Firmy z boostem trafiają wyżej w kolejce.

### Opinie i spory
Statusy: `Pending → Accepted / Rejected / UnderDispute`. Właściciel może zaakceptować lub zgłosić
opinię; wszystko, co pozostaje oczekujące ponad 7 dni, jest akceptowane automatycznie.

### Program poleceń
Kody poleceń, powiązania kont, typy celów (`ReviewsCompleted`, `BusinessVerified`,
`PointsEarned`, `OnboardingCompleted`), nagrody dla polecającego i bonus powitalny
(2 punkty) dla poleconego.

### Zgłoszenia (tickety)
System zgłoszeń z komentarzami, historią statusów, tematami i zamykaniem oraz godzinnym
cooldownem między kolejnymi zgłoszeniami. Operacje agenta i administratora są chronione
uprawnieniami.

### Utrzymanie i bezpieczeństwo
- **Rate limiting**: globalny i per-endpoint przez `[RateLimit(limit, periodInSeconds)]`,
  z nagłówkami `X-RateLimit-Limit`, `X-RateLimit-Remaining` i `X-RateLimit-Reset`.
- **Tryb konserwacji**: zapisany w bazie; `[DisableDuringMaintenance]` blokuje kontrolery,
  a posiadacze uprawnienia `BypassMaintenanceMode` mają dostęp.
- **Dashboard Hangfire**: chroniony uprawnieniem `AccessHangfireDashboard`.
- **Health check**: `GET /api/health`.
- **E-maile transakcyjne**: szablony powitalny, wprowadzenie do weryfikacji i przypomnienie
  o niedokończonej weryfikacji; przy wyłączonej poczcie działa `FakeEmailService`.

---

## Pierwsze uruchomienie

### Wymagania

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Docker i Docker Compose (dla MariaDB i Redisa)
- Projekt Google Cloud z włączonym **Places API** oraz **identyfikatorem klienta OAuth 2.0**

### 1. Klonowanie

```bash
git clone https://github.com/kkwlkk/RateRelay-Backend.git
```

> **Uwaga dla Windows:** ścieżki w repozytorium są bardzo długie. Jeśli checkout zakończy się
> błędem *„Filename too long”*, sklonuj z `git -c core.longpaths=true clone …`.

### 2. Uruchomienie infrastruktury

```bash
docker compose up -d
```

Startuje MariaDB na `127.0.0.1:3306` (baza `raterelay`) oraz Redis na `127.0.0.1:6379`, zgodnie
z domyślnymi wartościami w `appsettings.json`.

### 3. Dane dostępowe Google

Utwórz `RateRelay.API/.env` na podstawie szablonu:

```bash
cp RateRelay.API/.env.example RateRelay.API/.env
```

```dotenv
GoogleOAuth__ClientId=twoj-google-oauth-client-id
GoogleApis__ApiKey=twoj-klucz-google-places-api
```

Pliki `.env`, `.env.local` oraz `.env.{Środowisko}` są wczytywane automatycznie w każdym
środowisku innym niż produkcyjne. Na produkcji używaj zwykłych zmiennych środowiskowych.

### 4. Uruchomienie API

```bash
dotnet run --project RateRelay.API
```

Schemat bazy jest tworzony i aktualizowany przez **dbup** przy starcie, więc nie ma osobnego kroku
migracji. Swagger UI jest dostępny pod adresem:

```
http://localhost:5206/swagger
```

### Uruchomienie API w Dockerze

```bash
docker build -t raterelay-backend .
```

```bash
docker run -p 5000:5000 --env-file RateRelay.API/.env raterelay-backend
```

---

## Konfiguracja

Ustawienia znajdują się w `appsettings.json` i mogą być nadpisane przez
`appsettings.{Środowisko}.json`, zmienne środowiskowe, pliki `.env` oraz argumenty wiersza
poleceń (w tej kolejności pierwszeństwa).

| Sekcja | Przeznaczenie |
| --- | --- |
| `Database` | Connection string, hasło, limit czasu migracji (domyślnie 8 min) |
| `Redis` | Connection string i hasło |
| `Hangfire` | Prefiks kluczy, nazwa serwera, liczba workerów (domyślnie 20) |
| `Jwt` | Sekret, issuer, audience, czasy życia tokenów dostępu i odświeżania |
| `Encryption` | Klucz używany do szyfrowanych pól encji |
| `RateLimit` | Włącznik, limity domyślne i globalne, nazwy nagłówków odpowiedzi |
| `GoogleOAuth` / `GoogleApis` | Identyfikator klienta OAuth i klucz Places API |
| `Email` / `EmailLinks` / `Company` | Ustawienia SMTP i dane firmowe w szablonach e-mail |
| `AppLogger` | Katalog logów, interwał rotacji, przełączniki konsoli i plików |

> ⚠️ Wartości `Jwt:Secret` i `Encryption:Key` w `appsettings.json` to placeholdery.
> Wymień je przed jakimkolwiek realnym wdrożeniem.

---

## Przegląd API

Każda odpowiedź jest opakowana w kopertę:

```jsonc
{
  "success": true,
  "data": { /* … */ },
  "error": { "message": "…", "code": "…", "validationErrors": [] },
  "metadata": { /* stronicowanie, dodatkowy kontekst */ }
}
```

Ścieżki są zapisywane małymi literami, a JSON używa camelCase.

### Publiczne

| Metoda | Ścieżka | Opis |
| --- | --- | --- |
| `POST` | `/api/auth/google` | Logowanie tokenem Google ID |
| `POST` | `/api/auth/refresh-token` | Wymiana tokenu odświeżania na nowy token dostępu |
| `GET` | `/api/health` | Sprawdzenie stanu usługi |
| `GET` | `/api/maintenance` | Aktualny stan trybu konserwacji |

### Użytkownik: konto i onboarding

| Metoda | Ścieżka | Opis |
| --- | --- | --- |
| `GET` | `/api/user/account` | Dane bieżącego konta |
| `PATCH` | `/api/user/account/settings` | Aktualizacja ustawień i preferencji e-mail |
| `GET` | `/api/user/account/stats` | Statystyki konta |
| `GET` | `/api/user/account/reviews` | Historia wystawionych opinii (stronicowana) |
| `GET` | `/api/user/onboarding/status` | Postęp onboardingu |
| `POST` | `/api/user/onboarding/welcome` | Zakończenie kroku powitalnego |
| `POST` | `/api/user/onboarding/business-verification` | Zakończenie kroku weryfikacji |
| `POST` | `/api/user/onboarding/complete` | Zakończenie onboardingu |

### Użytkownik: firma i weryfikacja

| Metoda | Ścieżka | Opis |
| --- | --- | --- |
| `POST` | `/api/user/business/verification/initiate` | Zgłoszenie firmy przez Place ID |
| `GET` | `/api/user/business/verification/challenge` | Pobranie wyzwania z godzinami otwarcia |
| `POST` | `/api/user/business/verification/process` | Sprawdzenie wyzwania |
| `GET` | `/api/user/business/verification/status` | Status weryfikacji |
| `GET` | `/api/user/business` | Lista Twoich firm |
| `GET` | `/api/user/business/{businessId}` | Szczegóły firmy |
| `GET` | `/api/user/business/{businessId}/reviews` | Opinie o Twojej firmie |
| `POST` | `/api/user/business/{id}/reviews/{reviewId}/accept` | Akceptacja oczekującej opinii |
| `POST` | `/api/user/business/{id}/reviews/{reviewId}/report` | Zgłoszenie / zakwestionowanie opinii |

### Użytkownik: kolejka, polecenia, zgłoszenia

| Metoda | Ścieżka | Opis |
| --- | --- | --- |
| `GET` | `/api/user/reviewable-businesses/next` | Pobranie kolejnej firmy do oceny |
| `GET` | `/api/user/reviewable-businesses/time-left` | Czas pozostały na bieżący przydział |
| `POST` | `/api/user/reviewable-businesses/submit` | Wystawienie opinii |
| `GET` | `/api/user/referral/stats` | Statystyki poleceń |
| `GET` | `/api/user/referral/goals` | Cele poleceń i postęp |
| `POST` | `/api/user/referral/generate-code` | Wygenerowanie własnego kodu polecającego |
| `POST` | `/api/user/referral/link` | Powiązanie konta z kodem polecającym |
| `GET` `POST` | `/api/user/tickets` | Lista / utworzenie zgłoszenia |
| `GET` | `/api/user/tickets/{id}` | Szczegóły zgłoszenia (`?includeComments=true`) |
| `GET` `POST` | `/api/user/tickets/{id}/comments` | Odczyt / dodanie komentarzy |
| `PUT` | `/api/user/tickets/{id}/close` | Zamknięcie zgłoszenia |

### Administrator

Wszystkie ścieżki administracyjne wymagają konta administratora **oraz** wskazanego uprawnienia.

| Metoda | Ścieżka | Uprawnienie |
| --- | --- | --- |
| `GET` | `/api/admin/businesses` | `ViewAllBusinesses` |
| `POST` | `/api/admin/businesses` | `CreateBusiness` |
| `GET` | `/api/admin/businesses/{id}` | `ViewAllBusinesses` |
| `DELETE` | `/api/admin/businesses/{id}` | `DeleteBusiness` |
| `POST` | `/api/admin/businesses/{id}/boost` | `ManageBusinessPriority` |
| `POST` | `/api/admin/businesses/{id}/unboost` | `ManageBusinessPriority` |
| `GET` | `/api/admin/users` | `ViewAllUsers` |

---

## System punktów

| Stała | Wartość | Znaczenie |
| --- | --- | --- |
| `BasicReviewPoints` | 1 | Nagroda za zaakceptowaną opinię |
| `GoogleMapsReviewPoints` | 1 | Bonus za dodanie publicznej opinii w Google Maps |
| `MinimumOwnerPointBalanceForBusinessVisibility` | 2 | Saldo wymagane, by firma pozostała w kolejce |
| `ReferralWelcomeBonusPoints` | 2 | Bonus powitalny dla poleconego użytkownika |

Każdy ruch punktów trafia do tabeli `point_transactions` z typowanym powodem (blokada, nagroda,
zwrot, polecenie, korekta ręczna lub systemowa), dzięki czemu saldo ma pełną ścieżkę audytu.

---

## Zadania w tle

Cykliczne zadania Hangfire są wykrywane automatycznie po atrybucie `[HangfireRecurringJob]`:

| Zadanie | Harmonogram | Cel |
| --- | --- | --- |
| `AutoAcceptOverduePendingBusinessReviewsJob` | `5 0 * * *` (codziennie, 00:05) | Automatyczna akceptacja opinii oczekujących ponad 7 dni |
| `ExpiredBusinessVerificationsCleanupJob` | `5 */6 * * *` (co 6 h) | Usuwanie wygasłych wyzwań weryfikacyjnych |
| `ExpiredRefreshTokensCleanupJob` | `*/30 * * * *` (co 30 min) | Czyszczenie wygasłych tokenów odświeżania |
| `ExpiredBansCleanupJob` | `*/5 * * * *` (co 5 min) | Zdejmowanie wygasłych banów kont |

---

## Uprawnienia

Uprawnienia to flagi bitowe `ulong` przypisane do konta, sprawdzane przez `[RequirePermission(...)]`:

- **Zgłoszenia**: `ViewAllTickets`, `EditAllTickets`, `AssignTickets`, `ChangeTicketStatus`,
  `AddInternalComments`, `ViewInternalTicketData`, `HandleAssignedTickets`,
  `MarkTicketsObsolete`, `ViewTicketHistory`, `DeleteTickets`
- **Firmy**: `ViewAllBusinesses`, `CreateBusiness`, `DeleteBusiness`, `ManageBusinessPriority`
- **Użytkownicy**: `ViewAllUsers`, `ManageUsers`
- **System**: `AccessHangfireDashboard`, `ManageHangfireJobs`, `BypassMaintenanceMode`

---

## Struktura projektu

```
RateRelay-Backend/
├── RateRelay.API/                  # Warstwa HTTP
│   ├── Controllers/                # Auth, User/*, Admin/*, Health, Maintenance
│   ├── Attributes/                 # RequireAdmin, RequirePermission, RateLimit, …
│   ├── Middleware/                 # Obsługa wyjątków, rate limiting, logowanie IP
│   ├── Filters/                    # Tryb konserwacji, bezpieczeństwo Swaggera
│   ├── Program.cs / Startup.cs
│   └── appsettings.json
├── RateRelay.Application/          # Przypadki użycia
│   ├── Features/{Admin,User,Auth,Shared}/…   # Handlery CQRS i walidatory
│   ├── DTOs/                       # Kontrakty wejścia/wyjścia
│   ├── BackgroundJobs/             # Cykliczne zadania Hangfire
│   ├── MediatR/Behaviors/          # Logowanie, walidacja
│   └── Mapping/                    # Profile AutoMappera
├── RateRelay.Infrastructure/       # Implementacje techniczne
│   ├── DataAccess/                 # DbContext, repozytoria, UoW, Redis, migracje
│   ├── Services/                   # Auth, kolejka, opinie, punkty, polecenia, Google, e-mail
│   ├── Configuration/              # Klasy opcji (strongly-typed)
│   ├── EmailTemplates/             # Szablony .liquid
│   └── Hangfire/ · Logging/ · Authorization/
├── RateRelay.Domain/               # Encje, enumy, stałe, interfejsy
├── docker-compose.yml              # MariaDB + Redis
└── Dockerfile                      # Wieloetapowy build API
```

---

## Wdrożenie

- **Kontener**: wieloetapowy `Dockerfile` (build w SDK → runtime `aspnet:8.0`), port `5000`,
  wejście `dotnet RateRelay.API.dll`.
- **CI/CD**: `.github/workflows/deploy.yml` uruchamia się przy każdym pushu na `master`, działa
  na self-hosted runnerze, kopiuje drzewo robocze do katalogu wdrożeniowego i przebudowuje
  usługi compose `backend` oraz `nginx`.
- **Migracje**: stosowane automatycznie przy starcie; jeśli dbup zawiedzie, host nie wystartuje.
- **HTTPS/HSTS**: przekierowanie na HTTPS i HSTS są włączone w środowisku `Production`.
