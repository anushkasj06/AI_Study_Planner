import { useState } from 'react'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [submitting, setSubmitting] = useState(false)
  const { register, handleSubmit } = useForm<{ email: string; password: string }>({
    defaultValues: { email: 'demo@student.com', password: 'Demo@12345' }
  })

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <form
        onSubmit={handleSubmit(async (values) => {
          try {
            setSubmitting(true)
            await login(values)
            toast.success('Welcome back!')
            navigate('/app/dashboard')
          } catch (error) {
            toast.error((error as Error).message)
          } finally {
            setSubmitting(false)
          }
        })}
        className="glass-panel w-full max-w-md space-y-5 p-8"
      >
        <div>
          <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Welcome back</p>
          <h1 className="mt-2 text-3xl font-semibold text-ink">Log into your study workspace</h1>
        </div>
        <input {...register('email')} type="email" className="w-full rounded-2xl border-slate-200" placeholder="Email" />
        <input {...register('password')} type="password" className="w-full rounded-2xl border-slate-200" placeholder="Password" />
        <button disabled={submitting} className="w-full rounded-2xl bg-ink px-5 py-3 text-sm font-medium text-white">
          {submitting ? 'Signing in...' : 'Login'}
        </button>
        <p className="text-sm text-slate-500">
          Need an account? <Link to="/register" className="font-medium text-brand">Create one</Link>
        </p>
      </form>
    </div>
  )
}
