import { useEffect, useState } from 'react'
import toast from 'react-hot-toast'
import { studyPlannerApi } from '../api/studyPlannerApi'
import { ReminderForm } from '../components/reminders/ReminderForm'
import { EmptyState } from '../components/ui/EmptyState'
import { LoadingSpinner } from '../components/ui/LoadingSpinner'
import { formatDateTime } from '../lib/utils'
import type { Reminder } from '../types'

export function RemindersPage() {
  const [reminders, setReminders] = useState<Reminder[]>([])
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setReminders(await studyPlannerApi.reminders())
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

  if (loading) return <LoadingSpinner label="Loading reminders..." />

  return (
    <div className="grid gap-6 xl:grid-cols-[0.8fr_1.2fr]">
      <ReminderForm
        onSubmit={async (values) => {
          try {
            await studyPlannerApi.createReminder(values)
            toast.success('Reminder added')
            await load()
          } catch (error) {
            toast.error((error as Error).message)
          }
        }}
      />

      <div className="glass-panel p-5">
        <h2 className="text-2xl font-semibold text-ink">Your reminders</h2>
        <div className="mt-5 space-y-3">
          {reminders.length === 0 ? (
            <EmptyState title="No reminders scheduled" description="Create an in-app or email reminder for your next focused session." />
          ) : (
            reminders.map((reminder) => (
              <div key={reminder.id} className="rounded-2xl border border-slate-100 bg-slate-50 px-4 py-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="font-medium text-ink">{reminder.title}</p>
                    <p className="mt-1 text-sm text-slate-600">{reminder.message}</p>
                    <p className="mt-2 text-xs uppercase tracking-wide text-slate-400">{formatDateTime(reminder.reminderDateTime)}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    {!reminder.isRead && (
                      <button
                        onClick={async () => {
                          await studyPlannerApi.markReminderRead(reminder.id)
                          toast.success('Reminder marked as read')
                          await load()
                        }}
                        className="rounded-xl bg-sky-100 px-3 py-2 text-xs font-medium text-brand"
                      >
                        Mark read
                      </button>
                    )}
                    <button
                      onClick={async () => {
                        await studyPlannerApi.deleteReminder(reminder.id)
                        toast.success('Reminder deleted')
                        await load()
                      }}
                      className="rounded-xl bg-rose-100 px-3 py-2 text-xs font-medium text-rose-600"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  )
}
