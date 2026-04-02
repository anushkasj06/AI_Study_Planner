import { useEffect, useState } from 'react'
import toast from 'react-hot-toast'
import { useParams } from 'react-router-dom'
import { studyPlannerApi } from '../api/studyPlannerApi'
import { AIPlanPreview } from '../components/goals/AIPlanPreview'
import { StudyTaskList } from '../components/tasks/StudyTaskList'
import { EmptyState } from '../components/ui/EmptyState'
import { LoadingSpinner } from '../components/ui/LoadingSpinner'
import { formatDate } from '../lib/utils'
import type { StudyGoal, StudyPlan } from '../types'

export function GoalDetailsPage() {
  const { goalId = '' } = useParams()
  const [goal, setGoal] = useState<StudyGoal | null>(null)
  const [plans, setPlans] = useState<StudyPlan[]>([])
  const [loading, setLoading] = useState(true)

  const load = async () => {
    const [goalData, planData] = await Promise.all([studyPlannerApi.goalById(goalId), studyPlannerApi.plansByGoal(goalId)])
    setGoal(goalData)
    setPlans(planData)
  }

  useEffect(() => {
    const run = async () => {
      try {
        await load()
      } finally {
        setLoading(false)
      }
    }
    void run()
  }, [goalId])

  if (loading) return <LoadingSpinner label="Loading goal details..." />
  if (!goal) return <EmptyState title="Goal not found" description="The selected study goal could not be loaded." />

  return (
    <div className="space-y-6">
      <div className="glass-panel p-6">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Goal details</p>
            <h2 className="mt-2 text-3xl font-semibold text-ink">{goal.title}</h2>
            <p className="mt-3 max-w-3xl text-slate-600">{goal.description}</p>
          </div>
          <button
            onClick={async () => {
              try {
                await studyPlannerApi.generatePlan(goal.id, true)
                toast.success('Plan regenerated successfully')
                await load()
              } catch (error) {
                toast.error((error as Error).message)
              }
            }}
            className="rounded-2xl bg-ink px-5 py-3 text-sm font-medium text-white"
          >
            Regenerate plan
          </button>
        </div>

        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <Meta label="Target date" value={formatDate(goal.targetDate)} />
          <Meta label="Daily hours" value={`${goal.dailyAvailableHours} hrs`} />
          <Meta label="Status" value={goal.status} />
          <Meta label="Subjects" value={goal.subjects.join(', ')} />
        </div>
      </div>

      {plans.length > 0 ? (
        <>
          <AIPlanPreview plan={plans[0]} />
          <StudyTaskList title="Plan tasks" tasks={plans[0].tasks} onToggle={async (taskId) => { await studyPlannerApi.toggleTask(taskId); await load() }} />
        </>
      ) : (
        <EmptyState title="No plan yet" description="Generate a plan for this goal to start seeing structured study tasks." />
      )}
    </div>
  )
}

function Meta({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl bg-slate-50 px-4 py-4">
      <p className="text-xs uppercase tracking-wide text-slate-400">{label}</p>
      <p className="mt-2 text-sm font-medium text-ink">{value}</p>
    </div>
  )
}
