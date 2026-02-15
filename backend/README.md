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
