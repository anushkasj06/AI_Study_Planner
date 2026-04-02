import { useEffect, useState } from 'react'
import { Bar, BarChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { studyPlannerApi } from '../api/studyPlannerApi'
import { DashboardStats } from '../components/dashboard/DashboardStats'
import { ProgressCard } from '../components/dashboard/ProgressCard'
import { EmptyState } from '../components/ui/EmptyState'
import { LoadingSpinner } from '../components/ui/LoadingSpinner'
import { formatDateTime } from '../lib/utils'
import type { DashboardSummary, ProgressSummary } from '../types'

export function DashboardPage() {
  const [dashboard, setDashboard] = useState<DashboardSummary | null>(null)
  const [progress, setProgress] = useState<ProgressSummary | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const load = async () => {
      try {
        const [dashboardData, progressData] = await Promise.all([
          studyPlannerApi.dashboardSummary(),
          studyPlannerApi.progressSummary()
        ])
        setDashboard(dashboardData)
        setProgress(progressData)
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [])

  if (loading) return <LoadingSpinner label="Loading dashboard..." />
  if (!dashboard || !progress) return <EmptyState title="Dashboard unavailable" description="We could not load your study summary." />

  return (
    <div className="space-y-6">
      <DashboardStats
        items={[
          { label: 'Total goals', value: String(dashboard.totalGoals) },
          { label: 'Active goals', value: String(dashboard.activeGoals), tone: 'text-brand' },
          { label: 'Weekly study hours', value: `${dashboard.hoursStudiedThisWeek}h`, tone: 'text-amber-500' },
          { label: 'Current streak', value: `${dashboard.streakCount} days` }
        ]}
      />

      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <div className="glass-panel p-5">
          <h3 className="text-lg font-semibold text-ink">Goal completion overview</h3>
          <div className="mt-5 h-80">
            <ResponsiveContainer>
              <BarChart data={dashboard.progressByGoal}>
                <XAxis dataKey="goalTitle" hide />
                <YAxis />
                <Tooltip />
                <Bar dataKey="completionPercentage" fill="#0f766e" radius={[10, 10, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="space-y-6">
          <ProgressCard title="Weekly progress" value={progress.weeklyProgressPercentage} subtitle="Based on completed tasks." />
          <div className="glass-panel p-5">
            <h3 className="text-lg font-semibold text-ink">Upcoming reminders</h3>
            <div className="mt-4 space-y-3">
              {dashboard.upcomingReminders.length === 0 ? (
                <EmptyState title="No reminders yet" description="Create a reminder to keep your next session on track." />
              ) : (
                dashboard.upcomingReminders.map((item) => (
                  <div key={item.id} className="rounded-2xl border border-slate-100 bg-slate-50 px-4 py-3">
                    <p className="font-medium text-ink">{item.title}</p>
                    <p className="mt-1 text-sm text-slate-500">{formatDateTime(item.reminderDateTime)}</p>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
