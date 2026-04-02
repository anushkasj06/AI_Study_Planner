import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/layout/ProtectedRoute'
import { AppShell } from './components/layout/AppShell'
import { useAuth } from './context/AuthContext'
import { CreateGoalPage } from './pages/CreateGoalPage'
import { DashboardPage } from './pages/DashboardPage'
import { GoalDetailsPage } from './pages/GoalDetailsPage'
import { GoalsPage } from './pages/GoalsPage'
import { LandingPage } from './pages/LandingPage'
import { LoginPage } from './pages/LoginPage'
import { PlannerPage } from './pages/PlannerPage'
import { ProfilePage } from './pages/ProfilePage'
import { ProgressPage } from './pages/ProgressPage'
import { RegisterPage } from './pages/RegisterPage'
import { RemindersPage } from './pages/RemindersPage'

function App() {
  const { token } = useAuth()

  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/login" element={token ? <Navigate to="/app/dashboard" replace /> : <LoginPage />} />
      <Route path="/register" element={token ? <Navigate to="/app/dashboard" replace /> : <RegisterPage />} />

      <Route
        path="/app"
        element={
          <ProtectedRoute>
            <AppShell />
          </ProtectedRoute>
        }
      >
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="goals" element={<GoalsPage />} />
        <Route path="goals/new" element={<CreateGoalPage />} />
        <Route path="goals/:goalId" element={<GoalDetailsPage />} />
        <Route path="planner" element={<PlannerPage />} />
        <Route path="progress" element={<ProgressPage />} />
        <Route path="reminders" element={<RemindersPage />} />
        <Route path="profile" element={<ProfilePage />} />
        <Route index element={<Navigate to="/app/dashboard" replace />} />
      </Route>
    </Routes>
  )
}

export default App
