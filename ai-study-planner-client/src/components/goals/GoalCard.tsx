import { ArrowRight } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { StudyGoal } from '../../types'
import { formatDate } from '../../lib/utils'

export function GoalCard({ goal }: { goal: StudyGoal }) {
  return (
    <div className="glass-panel p-5">
      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="inline-flex rounded-full bg-sky-50 px-3 py-1 text-xs font-medium text-brand">{goal.priority}</div>
          <h3 className="mt-3 text-xl font-semibold text-ink">{goal.title}</h3>
          <p className="mt-2 text-sm text-slate-600">{goal.description}</p>
        </div>
        <div className="rounded-2xl bg-slate-100 px-3 py-2 text-sm text-slate-500">{goal.status}</div>
      </div>

      <div className="mt-5 grid gap-3 text-sm text-slate-600 sm:grid-cols-3">
        <div>
          <p className="text-xs uppercase tracking-wide text-slate-400">Target date</p>
          <p className="mt-1 font-medium text-ink">{formatDate(goal.targetDate)}</p>
        </div>
        <div>
          <p className="text-xs uppercase tracking-wide text-slate-400">Daily time</p>
          <p className="mt-1 font-medium text-ink">{goal.dailyAvailableHours} hrs</p>
        </div>
        <div>
          <p className="text-xs uppercase tracking-wide text-slate-400">Completion</p>
          <p className="mt-1 font-medium text-ink">{goal.completionPercentage}%</p>
        </div>
      </div>

      <div className="mt-5 h-2 overflow-hidden rounded-full bg-slate-100">
        <div className="h-full rounded-full bg-brand" style={{ width: `${goal.completionPercentage}%` }} />
      </div>

      <div className="mt-5 flex items-center justify-between">
        <div className="text-sm text-slate-500">{goal.subjects.join(', ')}</div>
        <Link to={`/app/goals/${goal.id}`} className="inline-flex items-center gap-2 text-sm font-medium text-brand">
          View details
          <ArrowRight className="h-4 w-4" />
        </Link>
      </div>
    </div>
  )
}
