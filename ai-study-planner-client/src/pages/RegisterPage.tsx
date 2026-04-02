import { useState } from 'react'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export function RegisterPage() {
  const { register: registerUser } = useAuth()
  const navigate = useNavigate()
  const [submitting, setSubmitting] = useState(false)
  const { register, handleSubmit } = useForm<{ fullName: string; email: string; password: string }>()

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <form
        onSubmit={handleSubmit(async (values) => {
          try {
            setSubmitting(true)
            await registerUser(values)
            toast.success('Account created successfully!')
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
          <p className="text-xs uppercase tracking-[0.3em] text-slate-400">New account</p>
          <h1 className="mt-2 text-3xl font-semibold text-ink">Create your planner profile</h1>
        </div>
        <input {...register('fullName')} className="w-full rounded-2xl border-slate-200" placeholder="Full name" />
        <input {...register('email')} type="email" className="w-full rounded-2xl border-slate-200" placeholder="Email" />
        <input {...register('password')} type="password" className="w-full rounded-2xl border-slate-200" placeholder="Password" />
        <button disabled={submitting} className="w-full rounded-2xl bg-ink px-5 py-3 text-sm font-medium text-white">
          {submitting ? 'Creating account...' : 'Register'}
        </button>
        <p className="text-sm text-slate-500">
          Already have an account? <Link to="/login" className="font-medium text-brand">Log in</Link>
        </p>
      </form>
    </div>
  )
}
