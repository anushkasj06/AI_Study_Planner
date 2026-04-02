import { ArrowRight, CheckCircle2, Sparkles } from 'lucide-react'
import { Link } from 'react-router-dom'

export function LandingPage() {
  return (
    <div className="min-h-screen bg-hero-grid">
      <div className="page-shell py-10">
        <header className="flex items-center justify-between">
          <div>
            <p className="text-xs uppercase tracking-[0.3em] text-slate-500">Portfolio Project</p>
            <h1 className="mt-2 text-3xl font-semibold text-ink">AI Study Planner for Students</h1>
          </div>
          <div className="flex gap-3">
            <Link to="/login" className="rounded-2xl border border-slate-200 px-4 py-2 text-sm font-medium text-ink">
              Login
            </Link>
            <Link to="/register" className="rounded-2xl bg-ink px-4 py-2 text-sm font-medium text-white">
              Get started
            </Link>
          </div>
        </header>

        <section className="mt-14 grid gap-10 lg:grid-cols-[1.2fr_0.8fr] lg:items-center">
          <div>
            <div className="inline-flex items-center gap-2 rounded-full bg-white/80 px-4 py-2 text-sm text-brand shadow-soft">
              <Sparkles className="h-4 w-4" />
              Gemini-powered schedules, reminders, and progress tracking
            </div>
            <h2 className="mt-6 text-5xl font-semibold leading-tight text-ink">
              Turn vague study goals into a realistic daily action plan.
            </h2>
            <p className="mt-6 max-w-2xl text-lg text-slate-600">
              Students can create goals, generate structured study plans, track progress, manage reminders, and build momentum with a clean dashboard.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Link to="/register" className="inline-flex items-center gap-2 rounded-2xl bg-ink px-5 py-3 text-sm font-medium text-white">
                Start planning
                <ArrowRight className="h-4 w-4" />
              </Link>
              <Link to="/login" className="rounded-2xl border border-slate-200 bg-white px-5 py-3 text-sm font-medium text-ink">
                Use demo account
              </Link>
            </div>
          </div>

          <div className="glass-panel p-6">
            <h3 className="text-xl font-semibold text-ink">What you can do</h3>
            <div className="mt-6 space-y-4">
              {[
                'Create semester, exam, or daily habit goals',
                'Generate AI study plans as structured tasks',
                'Track streaks, completion, and study hours',
                'Manage in-app reminders with optional email support'
              ].map((item) => (
                <div key={item} className="flex items-start gap-3 rounded-2xl bg-slate-50 px-4 py-4">
                  <CheckCircle2 className="mt-0.5 h-5 w-5 text-brand" />
                  <p className="text-sm text-slate-600">{item}</p>
                </div>
              ))}
            </div>
          </div>
        </section>
      </div>
    </div>
  )
}
