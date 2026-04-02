import { Clock3, Trash2 } from 'lucide-react'
import type { StudyTask } from '../../types'

export function TaskItem({
  task,
  onToggle,
  onDelete
}: {
  task: StudyTask
  onToggle?: (taskId: string) => Promise<void>
  onDelete?: (taskId: string) => Promise<void>
}) {
  return (
    <div className="rounded-2xl border border-slate-100 bg-white px-4 py-4 shadow-sm">
      <div className="flex items-start gap-4">
        <input
          type="checkbox"
          checked={task.isCompleted}
          onChange={() => void onToggle?.(task.id)}
          className="mt-1 rounded border-slate-300 text-brand focus:ring-brand"
        />
        <div className="flex-1">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h4 className="font-semibold text-ink">{task.topic}</h4>
              <p className="text-sm text-slate-500">{task.subtopic}</p>
            </div>
            <div className="flex items-center gap-3">
              <span className="rounded-full bg-sky-50 px-3 py-1 text-xs font-medium text-brand">{task.taskType}</span>
              {onDelete && (
                <button onClick={() => void onDelete(task.id)} className="text-slate-400 transition hover:text-rose-500">
                  <Trash2 className="h-4 w-4" />
                </button>
              )}
            </div>
          </div>
          <p className="mt-2 text-sm text-slate-600">{task.notes}</p>
          <div className="mt-3 flex flex-wrap items-center gap-4 text-xs text-slate-500">
            <div className="inline-flex items-center gap-1">
              <Clock3 className="h-3.5 w-3.5" />
              {task.estimatedHours}h planned
            </div>
            <div>{task.goalTitle}</div>
            <div>{task.priority} priority</div>
          </div>
        </div>
      </div>
    </div>
  )
}
