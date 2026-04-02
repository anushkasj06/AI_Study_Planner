import { useForm } from 'react-hook-form'
import type { Reminder, ReminderChannel } from '../../types'

interface ReminderFormValues {
  title: string
  message: string
  reminderDateTime: string
  channel: ReminderChannel
}

export function ReminderForm({
  initialReminder,
  onSubmit
}: {
  initialReminder?: Reminder
  onSubmit: (values: ReminderFormValues) => Promise<void>
}) {
  const { register, handleSubmit, reset } = useForm<ReminderFormValues>({
    defaultValues: initialReminder
      ? {
          title: initialReminder.title,
          message: initialReminder.message,
          reminderDateTime: initialReminder.reminderDateTime.slice(0, 16),
          channel: initialReminder.channel
        }
      : {
          channel: 'InApp'
        }
  })

  return (
    <form
      className="glass-panel space-y-4 p-5"
      onSubmit={handleSubmit(async (values) => {
        await onSubmit(values)
        reset()
      })}
    >
      <h3 className="text-lg font-semibold text-ink">{initialReminder ? 'Edit reminder' : 'Create reminder'}</h3>
      <input {...register('title')} placeholder="Study DBMS at 7 PM" className="w-full rounded-2xl border-slate-200" />
      <textarea {...register('message')} placeholder="Short reminder note" rows={3} className="w-full rounded-2xl border-slate-200" />
      <input type="datetime-local" {...register('reminderDateTime')} className="w-full rounded-2xl border-slate-200" />
      <select {...register('channel')} className="w-full rounded-2xl border-slate-200">
        <option value="InApp">In-app</option>
        <option value="Email">Email</option>
      </select>
      <button type="submit" className="rounded-2xl bg-ink px-5 py-3 text-sm font-medium text-white">
        {initialReminder ? 'Save changes' : 'Add reminder'}
      </button>
    </form>
  )
}
