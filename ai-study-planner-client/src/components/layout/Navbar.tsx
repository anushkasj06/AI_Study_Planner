import { Bell, Sparkles } from 'lucide-react'
import { useAuth } from '../../context/AuthContext'

export function Navbar() {
  const { user } = useAuth()

  return (
    <header className="glass-panel flex items-center justify-between px-5 py-4">
      <div>
        <p className="text-xs uppercase tracking-[0.3em] text-slate-400">AI Study Planner</p>
        <h1 className="mt-1 text-xl font-semibold text-ink">Make every study session count</h1>
      </div>
      <div className="flex items-center gap-3">
        <div className="hidden items-center gap-2 rounded-full bg-sky-50 px-4 py-2 text-sm text-slate-600 sm:flex">
          <Sparkles className="h-4 w-4 text-brand" />
          <span>AI scheduling ready</span>
        </div>
        <div className="flex h-11 w-11 items-center justify-center rounded-full bg-amber-100 text-amber-600">
          <Bell className="h-5 w-5" />
        </div>
        <div className="text-right">
          <p className="text-sm font-medium text-ink">{user?.fullName}</p>
          <p className="text-xs text-slate-500">{user?.email}</p>
        </div>
      </div>
    </header>
  )
}
