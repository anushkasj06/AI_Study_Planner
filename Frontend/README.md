# Frontend (AIStudyPlanner.Web)

ASP.NET MVC + Razor frontend for AI Study Planner.

## Key UX Features

- Secure login/register screens (phone included in register)
- Dashboard, goals, planner, progress, reminders, profile
- Goal deletion from list and details views
- Notification center integrated in top bar
- Floating AI assistant popup in bottom-right
- Assistant-driven note generation and mind map output preview
- The assistant panel now renders Mermaid mind maps from Groq-backed responses when available, and keeps a polished fallback graph when Groq is unavailable.
- Browser push subscription trigger from UI

## Setup

1. Configure env from `Frontend/.env.example`
2. Point API base URL to backend

```bash
cd Frontend
dotnet restore
dotnet run
```

## Configuration

`Api__BaseUrl` controls backend target.

Default local frontend URLs:

- `https://localhost:7102`
- `http://localhost:5102`

## Browser Push

Frontend registers `wwwroot/sw.js` service worker and subscribes the browser using the backend public VAPID key endpoint.
