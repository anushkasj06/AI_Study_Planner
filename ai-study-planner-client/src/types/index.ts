export type GoalStatus = 'Draft' | 'Active' | 'Completed' | 'Archived'
export type GoalPriority = 'Low' | 'Medium' | 'High'
export type DifficultyLevel = 'Beginner' | 'Intermediate' | 'Advanced'
export type PreferredStudyTime = 'Morning' | 'Afternoon' | 'Evening' | 'Flexible'
export type BreakPreference = 'Pomodoro' | 'ShortBreaks' | 'LongBreaks' | 'Minimal'
export type TaskType = 'Learn' | 'Revise' | 'Practice' | 'Test'
export type ReminderChannel = 'InApp' | 'Email'

export interface UserProfile {
  id: string
  fullName: string
  email: string
  createdAt: string
}

export interface AuthResponse {
  token: string
  user: UserProfile
}

export interface StudyGoal {
  id: string
  title: string
  description: string
  targetDate: string
  dailyAvailableHours: number
  difficultyLevel: DifficultyLevel
  priority: GoalPriority
  preferredStudyTime: PreferredStudyTime
  breakPreference: BreakPreference
  subjects: string[]
  status: GoalStatus
  totalTasks: number
  completedTasks: number
  completionPercentage: number
  createdAt: string
  updatedAt: string
}

export interface StudyTask {
  id: string
  studyPlanId: string
  studyGoalId: string
  taskDate: string
  topic: string
  subtopic: string
  estimatedHours: number
  actualHours: number
  taskType: TaskType
  notes: string
  priority: GoalPriority
  isCompleted: boolean
  completedAt?: string | null
  goalTitle: string
}

export interface StudyPlan {
  id: string
  studyGoalId: string
  title: string
  startDate: string
  endDate: string
  totalEstimatedHours: number
  generatedByAI: boolean
  createdAt: string
  tasks: StudyTask[]
}

export interface Reminder {
  id: string
  studyTaskId?: string | null
  title: string
  message: string
  reminderDateTime: string
  isSent: boolean
  isRead: boolean
  channel: ReminderChannel
}

export interface ProgressSummary {
  completedHoursThisWeek: number
  completedTasks: number
  totalTasks: number
  dailyStreak: number
  weeklyProgressPercentage: number
}

export interface GoalProgress {
  goalId: string
  goalTitle: string
  goalCompletionPercentage: number
  hoursSpent: number
  completedTasks: number
  totalTasks: number
}

export interface DashboardSummary {
  totalGoals: number
  activeGoals: number
  completedTasks: number
  pendingTasks: number
  hoursStudiedThisWeek: number
  streakCount: number
  upcomingReminders: Array<{
    id: string
    title: string
    reminderDateTime: string
    isRead: boolean
  }>
  progressByGoal: Array<{
    goalId: string
    goalTitle: string
    completionPercentage: number
  }>
}
