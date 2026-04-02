import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { studyPlannerApi } from '../api/studyPlannerApi'
import type { AuthResponse, UserProfile } from '../types'

interface AuthContextValue {
  token: string | null
  user: UserProfile | null
  isLoading: boolean
  login: (payload: { email: string; password: string }) => Promise<void>
  register: (payload: { fullName: string; email: string; password: string }) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('studyPlannerToken'))
  const [user, setUser] = useState<UserProfile | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const persistAuth = (data: AuthResponse) => {
    localStorage.setItem('studyPlannerToken', data.token)
    setToken(data.token)
    setUser(data.user)
  }

  const clearAuth = () => {
    localStorage.removeItem('studyPlannerToken')
    setToken(null)
    setUser(null)
  }

  useEffect(() => {
    const bootstrap = async () => {
      if (!token) {
        setIsLoading(false)
        return
      }

      try {
        setUser(await studyPlannerApi.me())
      } catch {
        clearAuth()
      } finally {
        setIsLoading(false)
      }
    }

    void bootstrap()
  }, [token])

  const value = useMemo<AuthContextValue>(
    () => ({
      token,
      user,
      isLoading,
      login: async (payload) => persistAuth(await studyPlannerApi.login(payload)),
      register: async (payload) => persistAuth(await studyPlannerApi.register(payload)),
      logout: clearAuth
    }),
    [token, user, isLoading]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }
  return context
}
