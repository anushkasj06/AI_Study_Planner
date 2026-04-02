# Frontend README

## Run

```powershell
cd ai-study-planner-client
npm install
npm run dev
```

## Environment

Create `.env`:

```env
VITE_API_BASE_URL=http://localhost:5000
```

## Pages Included

- Landing
- Login
- Register
- Dashboard
- Create Goal
- Goals List
- Goal Details
- Planner
- Progress
- Reminders
- Profile

## Notes

- JWT token is stored in local storage under `studyPlannerToken`.
- The frontend expects the backend CORS origin to allow `http://localhost:5173`.
