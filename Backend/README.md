# Backend (AIStudyPlanner.Api)

ASP.NET Core Web API for the AI Study Planner.

## What It Provides

- JWT authentication with account lockout and login rate limiting
- Goal, plan, task, progress, reminder, and dashboard APIs
- Assistant APIs for chat and AI-generated notes/mind maps
- In-app notification APIs
- Web push subscription APIs and push dispatch support
- Email reminder dispatch via SMTP
- Background reminder processor with smart reminder auto-generation

## Setup

1. Configure env from `Backend/.env.example`
2. Ensure MySQL is running
3. Apply database migration

```bash
cd Backend
dotnet restore
dotnet ef database update
dotnet run
```

## Important Config Keys

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpireMinutes`
- `Gemini__ApiKey`, `Gemini__Model`, `Gemini__RequestTimeoutSeconds`
- `Groq__ApiKey`, `Groq__Model`, `Groq__Endpoint`, `Groq__RequestTimeoutSeconds`
- `Frontend__BaseUrl`
- `Smtp__*`
- `WebPush__Subject`, `WebPush__PublicKey`, `WebPush__PrivateKey`

## Migration

Latest migration included:

- `AddAssistantNotificationsAndAuthHardening`

## Notes

- No demo account seeding is performed.
- AI provider chain is: Gemini -> Groq -> deterministic local plan fallback.
- Floating assistant chat and assistant note generation use the Groq-backed composer when Groq is configured; otherwise they fall back to a personalized local response and graph.
- Browser push requires valid VAPID keys and active user subscriptions.
