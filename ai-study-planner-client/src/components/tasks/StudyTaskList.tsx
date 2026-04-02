import type { StudyTask } from '../../types'
import { EmptyState } from '../ui/EmptyState'
import { TaskItem } from './TaskItem'

export function StudyTaskList({
  title,
  tasks,
  onToggle,
  onDelete
}: {
  title: string
  tasks: StudyTask[]
  onToggle?: (taskId: string) => Promise<void>
  onDelete?: (taskId: string) => Promise<void>
}) {
  return (
    <div className="glass-panel p-5">
      <h3 className="text-lg font-semibold text-ink">{title}</h3>
      <div className="mt-4 space-y-3">
        {tasks.length === 0 ? (
          <EmptyState title="No tasks yet" description="Generate a plan or create a goal to start seeing study sessions here." />
        ) : (
          tasks.map((task) => <TaskItem key={task.id} task={task} onToggle={onToggle} onDelete={onDelete} />)
        )}
      </div>
    </div>
  )
}
