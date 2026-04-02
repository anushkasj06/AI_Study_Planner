import { useForm } from 'react-hook-form'
import type { BreakPreference, DifficultyLevel, GoalPriority, GoalStatus, PreferredStudyTime, StudyGoal } from '../../types'

export interface GoalFormValues {
  title: string
  description: string
  targetDate: string
  dailyAvailableHours: number
  difficultyLevel: DifficultyLevel
  priority: GoalPriority
  preferredStudyTime: PreferredStudyTime
  breakPreference: BreakPreference
  subjects: string
  status: GoalStatus
  autoGeneratePlan: boolean
}

const difficultyOptions: DifficultyLevel[] = ['Beginner', 'Intermediate', 'Advanced']
const priorityOptions: GoalPriority[] = ['Low', 'Medium', 'High']
const studyTimeOptions: PreferredStudyTime[] = ['Morning', 'Afternoon', 'Evening', 'Flexible']
const breakOptions: BreakPreference[] = ['Pomodoro', 'ShortBreaks', 'LongBreaks', 'Minimal']
const statusOptions: GoalStatus[] = ['Draft', 'Active', 'Completed', 'Archived']

export function GoalForm({
  initialGoal,
  onSubmit,
  submitLabel
}: {
  initialGoal?: StudyGoal
  onSubmit: (values: GoalFormValues) => Promise<void>
  submitLabel: string
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<GoalFormValues>({
    defaultValues: initialGoal
      ? {
          title: initialGoal.title,
          description: initialGoal.description,
          targetDate: initialGoal.targetDate.slice(0, 10),
          dailyAvailableHours: initialGoal.dailyAvailableHours,
          difficultyLevel: initialGoal.difficultyLevel,
          priority: initialGoal.priority,
          preferredStudyTime: initialGoal.preferredStudyTime,
          breakPreference: initialGoal.breakPreference,
          subjects: initialGoal.subjects.join(', '),
          status: initialGoal.status,
          autoGeneratePlan: false
        }
      : {
          difficultyLevel: 'Intermediate',
          priority: 'High',
          preferredStudyTime: 'Evening',
          breakPreference: 'Pomodoro',
          status: 'Active',
          autoGeneratePlan: true
        }
  })

  return (
    <form className="glass-panel space-y-5 p-6" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid gap-5 md:grid-cols-2">
        <div className="md:col-span-2">
          <label className="mb-2 block text-sm font-medium text-ink">Goal title</label>
          <input {...register('title', { required: 'Title is required', minLength: 3 })} className="w-full rounded-2xl border-slate-200" placeholder="Complete DSA in 45 days" />
          {errors.title && <p className="mt-1 text-xs text-rose-500">{errors.title.message}</p>}
        </div>

        <div className="md:col-span-2">
          <label className="mb-2 block text-sm font-medium text-ink">Description</label>
          <textarea {...register('description', { required: 'Description is required', minLength: 10 })} rows={4} className="w-full rounded-2xl border-slate-200" />
          {errors.description && <p className="mt-1 text-xs text-rose-500">{errors.description.message}</p>}
        </div>

        <Field label="Target date"><input type="date" {...register('targetDate', { required: true })} className="w-full rounded-2xl border-slate-200" /></Field>
        <Field label="Daily hours"><input type="number" step="0.5" {...register('dailyAvailableHours', { valueAsNumber: true, min: 0.5, max: 16 })} className="w-full rounded-2xl border-slate-200" /></Field>
        <Field label="Difficulty">{select(register('difficultyLevel'), difficultyOptions)}</Field>
        <Field label="Priority">{select(register('priority'), priorityOptions)}</Field>
        <Field label="Preferred study time">{select(register('preferredStudyTime'), studyTimeOptions)}</Field>
        <Field label="Break preference">{select(register('breakPreference'), breakOptions)}</Field>
        <Field label="Status">{select(register('status'), statusOptions)}</Field>

        <div className="md:col-span-2">
          <label className="mb-2 block text-sm font-medium text-ink">Subjects / topics</label>
          <input {...register('subjects', { required: 'Add at least one subject' })} className="w-full rounded-2xl border-slate-200" placeholder="Arrays, DBMS, Java" />
        </div>
      </div>

      <label className="flex items-center gap-3 rounded-2xl bg-sky-50 px-4 py-3 text-sm text-slate-700">
        <input type="checkbox" {...register('autoGeneratePlan')} className="rounded border-slate-300" />
        Generate an AI study schedule right after saving this goal
      </label>

      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-2xl bg-ink px-5 py-3 text-sm font-medium text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-70"
      >
        {isSubmitting ? 'Saving...' : submitLabel}
      </button>
    </form>
  )
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="mb-2 block text-sm font-medium text-ink">{label}</label>
      {children}
    </div>
  )
}

function select(registerResult: ReturnType<typeof registerPlaceholder>, options: string[]) {
  return (
    <select {...registerResult} className="w-full rounded-2xl border-slate-200">
      {options.map((option) => (
        <option key={option} value={option}>
          {option}
        </option>
      ))}
    </select>
  )
}

function registerPlaceholder() {
  return { name: '' }
}
