import { Navigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { LoadingSpinner } from '../ui/LoadingSpinner'

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { token, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <LoadingSpinner label="Restoring your study workspace..." />
      </div>
    )
  }

  return token ? <>{children}</> : <Navigate to="/login" replace />
}
