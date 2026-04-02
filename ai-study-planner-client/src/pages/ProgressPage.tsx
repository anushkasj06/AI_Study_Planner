import { useEffect, useState } from 'react'
import { Pie, PieChart, Cell, ResponsiveContainer, Tooltip } from 'recharts'
import { studyPlannerApi } from '../api/studyPlannerApi'
import { EmptyState } from '../components/ui/EmptyState'
import { LoadingSpinner } from '../components/ui/LoadingSpinner'
import type { ProgressSummary, StudyGoal } from '../types'

export function ProgressPage() {
  const [summary, setSummary] = useState<ProgressSummary | null>(null)
  const [goals, setGoals] = useState<StudyGoal[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const load = async () => {
      try {
        const [summaryData, goalsData] = await Promise.all([studyPlannerApi.progressSummary(), studyPlannerApi.goals()])
        setSummary(summaryData)
        setGoals(goalsData)
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [])

  if (loading) return <LoadingSpinner label="Loading progress..." />
  if (!summary) return <EmptyState title="No progress yet" description="Start completing tasks to build progress insights." />

  const chartData = [
    { name: 'Completed', value: summary.completedTasks, color: '#0f766e' },
    { name: 'Pending', value: Math.max(summary.totalTasks - summary.completedTasks, 0), color: '#cbd5e1' }
  ]

  return (
    <div className="grid gap-6 xl:grid-cols-[0.9fr_1.1fr]">
      <div className="glass-panel p-5">
        <h2 className="text-2xl font-semibold text-ink">Progress summary</h2>
        <div className="mt-6 h-72">
          <ResponsiveContainer>
            <PieChart>
              <Pie data={chartData} innerRadius={75} outerRadius={110} paddingAngle={3} dataKey="value">
                {chartData.map((entry) => (
                  <Cell key={entry.name} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <Metric label="Hours this week" value={`${summary.completedHoursThisWeek}h`} />
          <Metric label="Daily streak" value={`${summary.dailyStreak} days`} />
          <Metric label="Completed tasks" value={`${summary.completedTasks}/${summary.totalTasks}`} />
          <Metric label="Weekly completion" value={`${summary.weeklyProgressPercentage}%`} />
        </div>
      </div>

      <div className="glass-panel p-5">
        <h3 className="text-lg font-semibold text-ink">Progress by goal</h3>
        <div className="mt-5 space-y-4">
          {goals.length === 0 ? (
            <EmptyState title="No goals yet" description="Create a goal to start tracking meaningful progress here." />
          ) : (
            goals.map((goal) => (
              <div key={goal.id} className="rounded-2xl border border-slate-100 bg-slate-50 px-4 py-4">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <p className="font-medium text-ink">{goal.title}</p>
                    <p className="text-sm text-slate-500">{goal.completedTasks} of {goal.totalTasks} tasks done</p>
                  </div>
                  <div className="text-sm font-medium text-brand">{goal.completionPercentage}%</div>
                </div>
                <div className="mt-3 h-2 overflow-hidden rounded-full bg-slate-200">
                  <div className="h-full rounded-full bg-brand" style={{ width: `${goal.completionPercentage}%` }} />
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  )
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl bg-slate-50 px-4 py-4">
      <p className="text-sm text-slate-500">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-ink">{value}</p>
    </div>
  )
}
