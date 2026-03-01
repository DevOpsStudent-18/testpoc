# EDF API POC

Simple .NET Web API proof-of-concept with an in-memory mock repository.

Quick start:

```bash
dotnet build "D:/dev work/EDF POC/backend/EDF.Api/EDF.Api.csproj"
dotnet run --project "D:/dev work/EDF POC/backend/EDF.Api/EDF.Api.csproj"
# from backend project folder
cd "d:\dev work\EDF POC\backend\EDF.Api"
dotnet publish -c Release -o ./publish
```

API endpoints:
- GET /api/devices
- GET /api/devices/{id}
- POST /api/devices
- PUT /api/devices/{id}
- DELETE /api/devices/{id}

Next steps: swap `InMemoryDeviceRepository` for a repository backed by Azure SQL.

## Using a real database

The project now supports an Azure SQL Database (or any SQL Server) via Entity Framework Core.

1. **local development** – the default connection string in `appsettings.json` uses `(localdb)\mssqllocaldb`. You can change it or add
   a new connection string in `appsettings.Development.json`.

2. **set your own connection string** – either update the file or pass a `ConnectionStrings__DefaultConnection`
   environment variable. When the value is empty the code falls back to the built‑in in‑memory repository.

3. **apply migrations** – EF will automatically run `Database.Migrate()` on startup. To create a migration locally run:

   ```bash
   dotnet tool install --global dotnet-ef # if you haven't already
   cd backend/EDF.Api
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Azure App Service deployment** – add a connection string under **Configuration > Connection strings** named
   `DefaultConnection` (the code reads `builder.Configuration.GetConnectionString("DefaultConnection")`).
   App Service also exposes these values as environment variables, so no changes are required in your code. If
   you prefer to use a normal App Setting key, you can read it by setting `ConnectionStrings__DefaultConnection`
   instead.

   > The API will automatically migrate the database when the App Service starts (requires the app to have
   > permission to modify the schema).
