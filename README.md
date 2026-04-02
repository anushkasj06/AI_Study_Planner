# AI Study Planner for Students

Full-stack portfolio project with:

- Backend: ASP.NET Core Web API (.NET 9, C#)
- ORM: Entity Framework Core
- Database: MySQL
- MySQL provider: Pomelo.EntityFrameworkCore.MySql
- Frontend: React + Vite + TypeScript
- Styling: Tailwind CSS
- Auth: JWT
- AI: Google Gemini API via `GEMINI_API_KEY` / `Gemini__ApiKey`

## Folder Structure

```text
study_planner/
├── .config/
│   └── dotnet-tools.json
├── AIStudyPlanner.Api/
│   ├── BackgroundServices/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Entities/
│   ├── Helpers/
│   ├── Interfaces/
│   ├── Middleware/
│   ├── Migrations/
│   ├── Services/
│   ├── Properties/
│   ├── .env.example
│   ├── AIStudyPlanner.Api.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── README.md
├── ai-study-planner-client/
│   ├── public/
│   ├── src/
│   │   ├── api/
│   │   ├── components/
│   │   ├── context/
│   │   ├── lib/
│   │   ├── pages/
│   │   ├── types/
│   │   ├── App.tsx
│   │   ├── index.css
│   │   └── main.tsx
│   ├── .env.example
│   ├── package.json
│   ├── postcss.config.js
│   ├── tailwind.config.js
│   ├── vite.config.ts
│   └── README.md
├── postman/
│   └── AIStudyPlanner.postman_collection.json
├── .gitignore
└── README.md
```

## Features

- User registration and login with JWT auth
- Study goal creation with structured metadata
- Gemini-powered study plan generation with fallback JSON
- Daily and weekly planner views
- Task completion and manual task management
- Progress tracking and dashboard summaries
- Reminder system with in-app reminders and optional SMTP email
- Demo seed data for local testing
- Swagger/OpenAPI for backend testing

## Prerequisites

- .NET SDK 9.0+
- Node.js 20+ and npm
- MySQL 8+
- A Gemini API key if you want live AI generation

## Configuration

### Backend config locations

- Database connection string:
  - `AIStudyPlanner.Api/appsettings.Development.json`
  - or environment variable `ConnectionStrings__DefaultConnection`
- JWT secret:
  - `AIStudyPlanner.Api/appsettings.Development.json`
  - or environment variable `Jwt__Key`
- Gemini API key:
  - `AIStudyPlanner.Api/appsettings.Development.json`
  - or environment variable `Gemini__ApiKey`
- Frontend allowed origin:
  - `AIStudyPlanner.Api/appsettings.Development.json`
  - or environment variable `Frontend__BaseUrl`

### Frontend config location

- API base URL:
  - `ai-study-planner-client/.env`
  - variable: `VITE_API_BASE_URL=http://localhost:5000`

## MySQL Setup

Create a local database:

```sql
CREATE DATABASE AIStudyPlannerDb;
```

Update the backend connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=AIStudyPlannerDb;user=root;password=YOUR_PASSWORD;"
}
```

Set a JWT key that is at least 32 characters long:

```json
"Jwt": {
  "Key": "ChangeThisToA32CharOrLongerJwtSecretKey123!"
}
```

## Backend Run Steps

```powershell
cd AIStudyPlanner.Api
dotnet restore
dotnet dotnet-ef database update
dotnet run
```

Backend URLs:

- API: `http://localhost:5000` or `https://localhost:7xxx`
- Swagger: `http://localhost:5000/swagger`

If you want to regenerate the migration from scratch later:

```powershell
dotnet dotnet-ef migrations add InitialCreate
dotnet dotnet-ef database update
```

## Frontend Run Steps

```powershell
cd ai-study-planner-client
npm install
npm run dev
```

Frontend URL:

- `http://localhost:5173`

## Demo Account

Seeded automatically on first backend startup:

- Email: `demo@student.com`
- Password: `Demo@12345`

## Environment Variables

Backend supports:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpireMinutes`
- `Gemini__ApiKey`
- `Gemini__Model`
- `Frontend__BaseUrl`
- `Smtp__Host`
- `Smtp__Port`
- `Smtp__Username`
- `Smtp__Password`
- `Smtp__FromEmail`

Frontend supports:

- `VITE_API_BASE_URL`

## Backend Packages Used

- `BCrypt.Net-Next`
- `FluentValidation.AspNetCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.EntityFrameworkCore.Design`
- `Pomelo.EntityFrameworkCore.MySql`
- `Swashbuckle.AspNetCore`

## Frontend Packages Used

- `react-router-dom`
- `axios`
- `react-hook-form`
- `@hookform/resolvers`
- `zod`
- `tailwindcss`
- `postcss`
- `autoprefixer`
- `recharts`
- `dayjs`
- `lucide-react`
- `react-hot-toast`
- `clsx`

## Key API Endpoints

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Goals

- `GET /api/goals`
- `GET /api/goals/{id}`
- `POST /api/goals`
- `PUT /api/goals/{id}`
- `DELETE /api/goals/{id}`

### AI / Plans / Tasks

- `POST /api/ai/generate-plan/{goalId}`
- `POST /api/ai/regenerate-plan/{goalId}`
- `GET /api/plans`
- `GET /api/plans/{id}`
- `GET /api/plans/goal/{goalId}`
- `GET /api/tasks/today`
- `GET /api/tasks/week`
- `PUT /api/tasks/{id}`
- `PATCH /api/tasks/{id}/toggle-complete`
- `DELETE /api/tasks/{id}`

### Progress

- `POST /api/progress/log`
- `GET /api/progress/summary`
- `GET /api/progress/goal/{goalId}`
- `GET /api/progress/streak`

### Reminders

- `GET /api/reminders`
- `POST /api/reminders`
- `PUT /api/reminders/{id}`
- `DELETE /api/reminders/{id}`
- `PATCH /api/reminders/{id}/mark-read`

### Dashboard

- `GET /api/dashboard/summary`

## Sample API Requests and Responses

### Register

Request:

```json
{
  "fullName": "Asha Student",
  "email": "asha@example.com",
  "password": "StrongPass123!"
}
```

Response:

```json
{
  "token": "JWT_TOKEN",
  "user": {
    "id": "GUID",
    "fullName": "Asha Student",
    "email": "asha@example.com",
    "createdAt": "2026-04-02T04:00:00Z"
  }
}
```

### Create Goal

Request:

```json
{
  "title": "Prepare for semester exams in 30 days",
  "description": "Focus on DBMS, CN, OS, and revision practice tests.",
  "targetDate": "2026-05-02",
  "dailyAvailableHours": 3,
  "difficultyLevel": "Intermediate",
  "priority": "High",
  "preferredStudyTime": "Evening",
  "breakPreference": "Pomodoro",
  "subjects": ["DBMS", "CN", "OS", "Revision"],
  "status": "Active",
  "autoGeneratePlan": true
}
```

Response:

```json
{
  "id": "GUID",
  "title": "Prepare for semester exams in 30 days",
  "description": "Focus on DBMS, CN, OS, and revision practice tests.",
  "targetDate": "2026-05-02T00:00:00Z",
  "dailyAvailableHours": 3,
  "difficultyLevel": "Intermediate",
  "priority": "High",
  "preferredStudyTime": "Evening",
  "breakPreference": "Pomodoro",
  "subjects": ["DBMS", "CN", "OS", "Revision"],
  "status": "Active",
  "totalTasks": 0,
  "completedTasks": 0,
  "completionPercentage": 0,
  "createdAt": "2026-04-02T04:00:00Z",
  "updatedAt": "2026-04-02T04:00:00Z"
}
```

### Dashboard Summary

Response:

```json
{
  "totalGoals": 1,
  "activeGoals": 1,
  "completedTasks": 0,
  "pendingTasks": 2,
  "hoursStudiedThisWeek": 1.5,
  "streakCount": 1,
  "upcomingReminders": [
    {
      "id": "GUID",
      "title": "Study Arrays at 7 PM",
      "reminderDateTime": "2026-04-02T12:00:00Z",
      "isRead": false
    }
  ],
  "progressByGoal": [
    {
      "goalId": "GUID",
      "goalTitle": "Complete DSA in 45 days",
      "completionPercentage": 0
    }
  ]
}
```

## Postman

Import:

- `postman/AIStudyPlanner.postman_collection.json`

Use a collection variable or manually paste the JWT token after login.

## Screenshots

Add your screenshots here after running the project:

- `docs/screenshots/landing-page.png`
- `docs/screenshots/dashboard.png`
- `docs/screenshots/goal-details.png`
- `docs/screenshots/planner.png`

## Common Error Fixes

- `Access denied for user 'root'@'localhost'`
  - Recheck MySQL username/password in `appsettings.Development.json` or `ConnectionStrings__DefaultConnection`.
- `401 Unauthorized`
  - Make sure you are logged in and the frontend still has a valid JWT in local storage.
- `500 during register/login with HS256 key size error`
  - Your `Jwt:Key` is too short.
  - Use at least 32 characters in `appsettings.Development.json` or `Jwt__Key`.
- `CORS error in browser`
  - Ensure backend `Frontend:BaseUrl` matches the frontend URL exactly, usually `http://localhost:5173`.
- `Gemini plan generation fails`
  - Add a valid API key in `Gemini__ApiKey`.
  - Without a key, the backend uses fallback sample JSON so the app still runs locally.
- `Email reminders do not send`
  - The app still works without SMTP. Configure `Smtp` settings only if you want email delivery.

## Final Checklist

- Backend compiles successfully
- Frontend builds successfully
- EF migration is included
- Demo seed data is included
- JWT auth is wired
- Swagger is enabled in development
- CORS is configured
- Gemini integration is included with fallback behavior
- Reminder background service is included
- README and env examples are included
