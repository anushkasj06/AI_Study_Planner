# Backend README

## Run

```powershell
cd AIStudyPlanner.Api
dotnet restore
dotnet dotnet-ef database update
dotnet run
```

## Important Config

- DB connection string: `appsettings.Development.json` or `ConnectionStrings__DefaultConnection`
- JWT secret: `appsettings.Development.json` or `Jwt__Key`
- Gemini API key: `appsettings.Development.json` or `Gemini__ApiKey`
- Frontend URL for CORS: `Frontend__BaseUrl`

## Seeded Demo User

- Email: `demo@student.com`
- Password: `Demo@12345`

## Notes

- The app auto-runs pending migrations on startup.
- The reminder worker keeps running even if SMTP is not configured.
- Gemini falls back to sample JSON when no API key is present.
