import { useEffect, useState } from 'react'
import toast from 'react-hot-toast'
import { studyPlannerApi } from '../api/studyPlannerApi'
import { StudyTaskList } from '../components/tasks/StudyTaskList'
import { WeeklyPlanner } from '../components/tasks/WeeklyPlanner'
import { LoadingSpinner } from '../components/ui/LoadingSpinner'
import type { StudyTask } from '../types'

export function PlannerPage() {
  const [todayTasks, setTodayTasks] = useState<StudyTask[]>([])
  const [weekTasks, setWeekTasks] = useState<StudyTask[]>([])
  const [loading, setLoading] = useState(true)

  const load = async () => {
    const [today, week] = await Promise.all([studyPlannerApi.todayTasks(), studyPlannerApi.weekTasks()])
    setTodayTasks(today)
    setWeekTasks(week)
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
  }, [])

  if (loading) return <LoadingSpinner label="Loading planner..." />

  const handleToggle = async (taskId: string) => {
    try {
      await studyPlannerApi.toggleTask(taskId)
      toast.success('Task updated')
      await load()
    } catch (error) {
      toast.error((error as Error).message)
    }
  }

  const handleDelete = async (taskId: string) => {
    try {
      await studyPlannerApi.deleteTask(taskId)
      toast.success('Task removed')
      await load()
    } catch (error) {
      toast.error((error as Error).message)
    }
  }

  return (
    <div className="space-y-6">
      <WeeklyPlanner tasks={weekTasks} />
      <StudyTaskList title="Today's focus tasks" tasks={todayTasks} onToggle={handleToggle} onDelete={handleDelete} />
    </div>
  )
}
