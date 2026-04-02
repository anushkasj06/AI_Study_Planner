import toast from 'react-hot-toast'
import { useNavigate } from 'react-router-dom'
import { studyPlannerApi } from '../api/studyPlannerApi'
import { GoalForm, type GoalFormValues } from '../components/goals/GoalForm'

export function CreateGoalPage() {
  const navigate = useNavigate()

  const handleSubmit = async (values: GoalFormValues) => {
    const goal = await studyPlannerApi.createGoal({
      ...values,
      subjects: values.subjects.split(',').map((item) => item.trim()).filter(Boolean)
    })
    toast.success('Goal created successfully')
    navigate(`/app/goals/${goal.id}`)
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-semibold text-ink">Create a new study goal</h2>
        <p className="mt-1 text-sm text-slate-500">Add the goal details that Gemini will use to produce a structured schedule.</p>
      </div>
      <GoalForm onSubmit={handleSubmit} submitLabel="Save goal" />
    </div>
  )
}
