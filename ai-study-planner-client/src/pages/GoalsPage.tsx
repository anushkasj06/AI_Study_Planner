import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { studyPlannerApi } from '../api/studyPlannerApi'
import { GoalCard } from '../components/goals/GoalCard'
import { EmptyState } from '../components/ui/EmptyState'
import { LoadingSpinner } from '../components/ui/LoadingSpinner'
import type { StudyGoal } from '../types'

export function GoalsPage() {
  const [goals, setGoals] = useState<StudyGoal[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const load = async () => {
      try {
        setGoals(await studyPlannerApi.goals())
      } finally {
        setLoading(false)
      }
    }
    void load()
  }, [])

  if (loading) return <LoadingSpinner label="Loading goals..." />

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-semibold text-ink">Study goals</h2>
          <p className="mt-1 text-sm text-slate-500">Manage your deadlines, priorities, and AI-generated plans.</p>
        </div>
        <Link to="/app/goals/new" className="rounded-2xl bg-ink px-5 py-3 text-sm font-medium text-white">
          Create goal
        </Link>
      </div>

      {goals.length === 0 ? (
        <EmptyState title="No goals yet" description="Create your first goal and generate an AI study plan in one flow." />
      ) : (
        <div className="grid gap-5 xl:grid-cols-2">
          {goals.map((goal) => (
            <GoalCard key={goal.id} goal={goal} />
          ))}
        </div>
      )}
    </div>
  )
}
