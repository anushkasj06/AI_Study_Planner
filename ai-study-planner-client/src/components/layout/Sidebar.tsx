import { BarChart3, CalendarDays, ClipboardList, Home, PlusCircle, TimerReset, UserCircle2 } from 'lucide-react'
import { NavLink } from 'react-router-dom'
import { cn } from '../../lib/utils'

const navItems = [
  { to: '/app/dashboard', label: 'Dashboard', icon: Home },
  { to: '/app/goals', label: 'Goals', icon: ClipboardList },
  { to: '/app/goals/new', label: 'Create Goal', icon: PlusCircle },
  { to: '/app/planner', label: 'Planner', icon: CalendarDays },
  { to: '/app/progress', label: 'Progress', icon: BarChart3 },
  { to: '/app/reminders', label: 'Reminders', icon: TimerReset },
  { to: '/app/profile', label: 'Profile', icon: UserCircle2 }
]

export function Sidebar() {
  return (
    <aside className="glass-panel flex h-full flex-col p-4">
      <div className="rounded-3xl bg-hero-grid p-5">
        <p className="text-xs uppercase tracking-[0.3em] text-slate-500">Workspace</p>
        <h2 className="mt-2 text-2xl font-semibold text-ink">Focus mode</h2>
        <p className="mt-2 text-sm text-slate-600">Plan smart, revise consistently, and stay ahead of deadlines.</p>
      </div>

      <nav className="mt-6 space-y-2">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium transition',
                isActive ? 'bg-ink text-white shadow-soft' : 'text-slate-600 hover:bg-slate-100'
              )
            }
          >
            <item.icon className="h-4 w-4" />
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}
