# Frontend README

This folder now contains the ASP.NET MVC frontend that replaces the React app.

## Run

```powershell
dotnet run
```

## Configuration

Edit `appsettings.json` if your API runs on a different URL.

```json
"Api": {
	"BaseUrl": "https://localhost:7001/"
}
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

- Authentication is stored in a cookie session in the ASP.NET frontend.
- The frontend calls the backend API with the JWT returned from login/register.
