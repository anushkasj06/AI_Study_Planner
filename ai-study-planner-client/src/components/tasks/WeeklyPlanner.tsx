import dayjs from 'dayjs'
import type { StudyTask } from '../../types'

export function WeeklyPlanner({ tasks }: { tasks: StudyTask[] }) {
  const grouped = tasks.reduce<Record<string, StudyTask[]>>((acc, task) => {
    const key = task.taskDate.slice(0, 10)
    acc[key] ??= []
    acc[key].push(task)
    return acc
  }, {})

  return (
    <div className="glass-panel p-5">
      <h3 className="text-lg font-semibold text-ink">Weekly planner</h3>
      <div className="mt-4 grid gap-4 xl:grid-cols-7">
        {Array.from({ length: 7 }).map((_, index) => {
          const day = dayjs().startOf('week').add(index, 'day')
          const key = day.format('YYYY-MM-DD')
          return (
            <div key={key} className="rounded-2xl border border-slate-100 bg-slate-50 p-3">
              <p className="text-xs uppercase tracking-wide text-slate-400">{day.format('ddd')}</p>
              <p className="mt-1 font-semibold text-ink">{day.format('DD MMM')}</p>
              <div className="mt-3 space-y-2">
                {(grouped[key] ?? []).map((task) => (
                  <div key={task.id} className="rounded-xl bg-white px-3 py-2 text-xs text-slate-600 shadow-sm">
                    <p className="font-medium text-ink">{task.topic}</p>
                    <p>{task.estimatedHours} hrs</p>
                  </div>
                ))}
                {(grouped[key] ?? []).length === 0 && <p className="text-xs text-slate-400">Rest / buffer</p>}
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
