# EventBooking API

REST API za upravljanje događajima i rezervacijama, izgrađen na .NET 8 sa Clean Architecture i CQRS principima.

---

## Tehnologije

| Sloj | Tehnologije |
|---|---|
| API | ASP.NET Core 8, Swagger/OpenAPI |
| Application | MediatR 14 (CQRS), FluentValidation |
| Domain | DDD factory metode, private setteri |
| Infrastructure | Entity Framework Core 8, SQL Server |
| Auth | JWT Bearer, SHA-256 hashing |
| Testovi | xUnit, Moq, MockQueryable |

---

## Arhitektura

```
EventBooking.Domain          → entiteti, bez zavisnosti
EventBooking.Application     → handleri, interfejsi, validatori
EventBooking.Infrastructure  → EF Core, repozitoriji, JWT, email
EventBooking.Api             → kontroleri, DI wiring, Swagger
EventBooking.Common          → deljeni API modeli

tests/
  EventBooking.Domain.Tests        → unit testovi entiteta
  EventBooking.Application.Tests   → unit testovi handlera (Moq)
```

### CQRS + Repository pattern

- **Query handleri** direktno koriste `IApplicationDbContext` za projekcije sa `AsNoTracking()`
- **Command handleri** koriste `IEventRepository` / `IReservationRepository` za pristup podacima i `IUnitOfWork` za čuvanje

---

## Pokretanje

### Preduslovi

- .NET 8 SDK
- SQL Server ili LocalDB

### 1. Kloniranje i restore

```bash
git clone <repo-url>
cd Events/src/EventBooking.Api
dotnet restore
```

### 2. Konfiguracija baze

`EventBooking.Api/appsettings.json` — podrazumevana konekcija:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EventBookingDb;Trusted_Connection=True;"
}
```

### 3. Migracija baze

```bash
dotnet ef database update --project EventBooking.Infrastructure --startup-project EventBooking.Api
```

Migracija automatski kreira:
- Tabele: `Events`, `Reservations`, `Users`, `Locations`, `Categories`, `ReservationStatuses`
- Seed data: 6 kategorija, 4 statusa rezervacije, admin korisnik

### 4. Pokretanje API-ja

```bash
dotnet run --project EventBooking.Api
```

Swagger UI: `https://localhost:{port}/swagger`

---

## Autentikacija

API koristi JWT Bearer autentikaciju.

**Podrazumevani admin nalog (seed):**

| Polje | Vrednost |
|---|---|
| Username | `admin` |
| Password | `Admin123!` |
| Email | `admin@eventbooking.com` |

**Tok autentikacije:**

```
POST /api/auth/login
→ { "token": "eyJ..." }

Swagger → Authorize → Bearer eyJ...
```

### Role

| Rola | Pristup |
|---|---|
| `Admin` | CRUD događaji, lokacije, odobravanje rezervacija |
| `User` | Kreiranje i otkazivanje rezervacija |
| Anonimno | Pregled događaja, lokacija, kategorija |

---

## API Endpointi

### Auth
| Metoda | Ruta | Opis |
|---|---|---|
| POST | `/api/auth/login` | Prijava, vraća JWT token |

### Događaji
| Metoda | Ruta | Auth | Opis |
|---|---|---|---|
| GET | `/api/events` | – | Lista događaja (filter + sort) |
| GET | `/api/events/{id}` | – | Detalji događaja |
| POST | `/api/events` | Admin | Kreiranje događaja |
| PUT | `/api/events/{id}` | Admin | Izmena događaja |
| DELETE | `/api/events/{id}` | Admin | Deaktivacija (soft delete) |

**Query parametri za GET /api/events:**
`city`, `categoryId`, `dateFrom`, `dateTo`, `includeInactive`, `sortBy` (Title/BasePrice/Capacity/City/EventDate), `sortDescending`

### Rezervacije
| Metoda | Ruta | Auth | Opis |
|---|---|---|---|
| GET | `/api/reservations` | Admin | Lista rezervacija |
| GET | `/api/reservations/{id}` | Admin | Detalji rezervacije |
| POST | `/api/reservations` | User | Kreiranje rezervacije |
| PUT | `/api/reservations/{id}` | User | Izmena sedišta (samo PENDING) |
| DELETE | `/api/reservations/{id}` | User | Otkazivanje rezervacije |
| POST | `/api/reservations/{id}/approve` | Admin | Odobravanje rezervacije |

### Lokacije i kategorije
| Metoda | Ruta | Auth | Opis |
|---|---|---|---|
| GET | `/api/locations` | – | Lista lokacija |
| POST | `/api/locations` | Admin | Kreiranje lokacije |
| GET | `/api/categories` | – | Lista kategorija |

---

## Biznis logika

### Kapacitet i lista čekanja

Kada korisnik kreira rezervaciju:
- Ako ima dovoljno slobodnih mesta → status `PENDING`
- Ako nema dovoljno mesta → status `WAITLIST`

Kada se rezervacija otkaže (`PENDING` ili `CONFIRMED`):
- Sistem automatski pronalazi najstariju `WAITLIST` rezervaciju čiji broj sedišta staje u oslobođeni kapacitet
- Ta rezervacija se automatski odobrava i korisniku se šalje email notifikacija

> Otkazivanje `WAITLIST` rezervacije **ne** okida promociju ostalih — ta sedišta nikad nisu zauzela kapacitet.

### Statusovi rezervacije

```
PENDING → CONFIRMED (odobravanje)
PENDING → CANCELLED (otkazivanje)
CONFIRMED → CANCELLED (otkazivanje)
WAITLIST → CONFIRMED (automatska promocija)
WAITLIST → CANCELLED (otkazivanje)
```

### Email notifikacije

Email se šalje pri odobravanju rezervacije. Template se konfiguriše u `appsettings.json`:

```json
"EmailTemplates": {
  "ReservationApproved": {
    "Subject": "Rezervacija odobrena – {EventTitle}",
    "Body": "... {EventTitle} {ReservationDate} {SeatNumbers} {ReservationId}"
  }
}
```

Trenutna implementacija loguje email (`EmailQueueService`) — zameniti sa stvarnim queue/SMTP servisom.

---

## Testovi

```bash
dotnet test
```

| Projekat | Testovi | Tip |
|---|---|---|
| `EventBooking.Domain.Tests` | 10 | Unit — čiste metode entiteta |
| `EventBooking.Application.Tests` | 16 | Unit — handleri sa Moq repozitorijumima |

**Pokriveno:**
- `Event` — Create, Update, Deactivate
- `Reservation` — Create, CreateWaitlist, Approve, Cancel, Update
- `CreateEventCommandHandler` — validacija lokacije i kategorije
- `CreateReservationCommandHandler` — PENDING vs WAITLIST logika
- `CancelReservationCommandHandler` — otkazivanje, WAITLIST ne okida event
- `ApproveReservationCommandHandler` — odobravanje, validacija email-a i statusa

---

## Struktura baze

```
Locations (Id, City, PostalCode)
Categories (Id, Name)
Users (Id, UserName, Email, PasswordHash, Role)
Events (Id, Title, Description, LocationId→, CategoryId→, EventDate, BasePrice, Capacity, CreatedAt, IsActive)
ReservationStatuses (Id, Code, Name)
Reservations (Id, EventId→, UserId→, StatusCode→, SeatNumbers[JSON], SeatCount, ReservationDate)
```
