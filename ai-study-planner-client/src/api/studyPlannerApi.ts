import { api } from './client'
import type {
  AuthResponse,
  DashboardSummary,
  GoalProgress,
  ProgressSummary,
  Reminder,
  StudyGoal,
  StudyPlan,
  StudyTask,
  UserProfile
} from '../types'

export const studyPlannerApi = {
  register: async (payload: { fullName: string; email: string; password: string }) =>
    (await api.post<AuthResponse>('/auth/register', payload)).data,
  login: async (payload: { email: string; password: string }) =>
    (await api.post<AuthResponse>('/auth/login', payload)).data,
  me: async () => (await api.get<UserProfile>('/auth/me')).data,
  goals: async () => (await api.get<StudyGoal[]>('/goals')).data,
  goalById: async (goalId: string) => (await api.get<StudyGoal>(`/goals/${goalId}`)).data,
  createGoal: async (payload: Record<string, unknown>) => (await api.post<StudyGoal>('/goals', payload)).data,
  updateGoal: async (goalId: string, payload: Record<string, unknown>) =>
    (await api.put<StudyGoal>(`/goals/${goalId}`, payload)).data,
  deleteGoal: async (goalId: string) => api.delete(`/goals/${goalId}`),
  generatePlan: async (goalId: string, regenerate = false) =>
    (await api.post<StudyPlan>(`/ai/${regenerate ? 'regenerate-plan' : 'generate-plan'}/${goalId}`)).data,
  plans: async () => (await api.get<StudyPlan[]>('/plans')).data,
  plansByGoal: async (goalId: string) => (await api.get<StudyPlan[]>(`/plans/goal/${goalId}`)).data,
  todayTasks: async () => (await api.get<StudyTask[]>('/tasks/today')).data,
  weekTasks: async () => (await api.get<StudyTask[]>('/tasks/week')).data,
  updateTask: async (taskId: string, payload: Record<string, unknown>) =>
    (await api.put<StudyTask>(`/tasks/${taskId}`, payload)).data,
  toggleTask: async (taskId: string) => (await api.patch<StudyTask>(`/tasks/${taskId}/toggle-complete`)).data,
  deleteTask: async (taskId: string) => api.delete(`/tasks/${taskId}`),
  logProgress: async (payload: Record<string, unknown>) => (await api.post('/progress/log', payload)).data,
  progressSummary: async () => (await api.get<ProgressSummary>('/progress/summary')).data,
  goalProgress: async (goalId: string) => (await api.get<GoalProgress>(`/progress/goal/${goalId}`)).data,
  streak: async () => (await api.get<{ currentStreak: number }>('/progress/streak')).data,
  reminders: async () => (await api.get<Reminder[]>('/reminders')).data,
  createReminder: async (payload: object) => (await api.post<Reminder>('/reminders', payload)).data,
  updateReminder: async (reminderId: string, payload: object) =>
    (await api.put<Reminder>(`/reminders/${reminderId}`, payload)).data,
  deleteReminder: async (reminderId: string) => api.delete(`/reminders/${reminderId}`),
  markReminderRead: async (reminderId: string) =>
    (await api.patch<Reminder>(`/reminders/${reminderId}/mark-read`)).data,
  dashboardSummary: async () => (await api.get<DashboardSummary>('/dashboard/summary')).data
}
