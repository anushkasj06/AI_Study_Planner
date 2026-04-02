import type { StudyPlan } from '../../types'
import { formatDate } from '../../lib/utils'

export function AIPlanPreview({ plan }: { plan: StudyPlan }) {
  return (
    <div className="glass-panel p-5">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs uppercase tracking-[0.3em] text-slate-400">AI Plan Preview</p>
          <h3 className="mt-2 text-xl font-semibold text-ink">{plan.title}</h3>
          <p className="mt-2 text-sm text-slate-500">
            {formatDate(plan.startDate)} to {formatDate(plan.endDate)} • {plan.totalEstimatedHours} estimated hours
          </p>
        </div>
        <div className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-700">
          {plan.generatedByAI ? 'Gemini generated' : 'Fallback generated'}
        </div>
      </div>

      <div className="mt-5 space-y-3">
        {plan.tasks.slice(0, 5).map((task) => (
          <div key={task.id} className="rounded-2xl border border-slate-100 bg-slate-50 px-4 py-3">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="font-medium text-ink">{task.topic}</p>
                <p className="text-sm text-slate-500">{task.subtopic}</p>
              </div>
              <div className="text-right text-sm text-slate-500">
                <p>{formatDate(task.taskDate)}</p>
                <p>{task.estimatedHours} hrs</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
