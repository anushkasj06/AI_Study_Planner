# AI Study Planner

Production-oriented full-stack study planner built with ASP.NET:

- Backend: ASP.NET Core Web API (.NET 10)
- Frontend: ASP.NET Core MVC + Razor (.NET 10)
- Database: MySQL + EF Core
- AI: Gemini-backed scheduling with resilient fallback
- Notifications: In-app + email + browser push (VAPID)

## Highlights

- Secure register/login with JWT, account lockout, and login throttling
- Registration requires phone number
- Goal lifecycle support including create, view, regenerate plan, and delete
- Intelligent planning based on goal inputs plus recent user progress context
- Floating AI assistant chat widget with user-context-aware replies
- AI note generation + mind map output (Mermaid syntax)
- Reminder automation with AI-generated smart reminder seeding
- Notification center with unread tracking and mark-read actions
- Browser push subscription support via service worker

## Workspace Layout

- `Backend/`: API project (`AIStudyPlanner.Api.csproj`)
- `Frontend/`: MVC web project (`AIStudyPlanner.Web.csproj`)
- `postman/`: Postman collection

## Environment Setup

Use `.env` files (not committed) and the provided examples:

- `Backend/.env.example`
- `Frontend/.env.example`

Copy each example to `.env` and fill values.

Required backend env keys:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpireMinutes`
- `Gemini__ApiKey`, `Gemini__Model`
- `Frontend__BaseUrl`
- `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`, `Smtp__FromEmail`
- `WebPush__Subject`, `WebPush__PublicKey`, `WebPush__PrivateKey`

Required frontend env keys:

- `Api__BaseUrl`

## Run

1. Backend

```bash
cd Backend
dotnet restore
dotnet ef database update
dotnet run
```

2. Frontend

```bash
cd Frontend
dotnet restore
dotnet run
```

Default local URLs:

- Backend: `https://localhost:7001` / `http://localhost:5000`
- Frontend: `https://localhost:7102` / `http://localhost:5102`

## Browser Push Notes

Browser notifications require:

- a push subscription created from the frontend session
- valid VAPID keys configured in backend env
- a supported browser with notification permission granted

Push delivery can work while the site tab is closed, but only after the user has subscribed at least once from that browser profile.

## Migration Included

This workspace includes migration:

- `AddAssistantNotificationsAndAuthHardening`

It adds:

- user phone/login-hardening fields
- notifications table
- web push subscriptions table
- study notes table

## Security Notes

- Do not commit `.env` files with secrets.
- Replace all example keys/passwords before deployment.
- Prefer HTTPS only in production and rotate JWT/WebPush keys regularly.
