# SettleMate tests

## Unit tests

Unit tests are isolated and do not require Docker:

```powershell
dotnet test SettleMate.Tests\SettleMate.Tests.csproj --filter FullyQualifiedName~SettleMate.Tests.Unit
```

## Integration tests

Integration tests use `WebApplicationFactory` and a SQL Server Testcontainer. Docker Desktop or another Docker Engine must be running:

```powershell
dotnet test SettleMate.Tests\SettleMate.Tests.csproj --filter Category=Integration
```

The fixture starts `mcr.microsoft.com/mssql/server:2022-latest`, applies the application's migrations, and uses a test authentication scheme. Integration tests should assert the HTTP response and persisted state through a fresh dependency-injection scope.
