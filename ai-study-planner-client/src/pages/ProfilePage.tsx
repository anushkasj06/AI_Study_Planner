import { useAuth } from '../context/AuthContext'

export function ProfilePage() {
  const { user, logout } = useAuth()

  return (
    <div className="glass-panel max-w-3xl p-6">
      <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Profile</p>
      <h2 className="mt-2 text-3xl font-semibold text-ink">{user?.fullName}</h2>
      <p className="mt-2 text-slate-500">{user?.email}</p>

      <div className="mt-6 grid gap-4 sm:grid-cols-2">
        <InfoCard label="Member since" value={user?.createdAt ? new Date(user.createdAt).toLocaleDateString('en-IN') : '-'} />
        <InfoCard label="Auth mode" value="JWT bearer token" />
      </div>

      <button onClick={logout} className="mt-6 rounded-2xl bg-ink px-5 py-3 text-sm font-medium text-white">
        Logout
      </button>
    </div>
  )
}

function InfoCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl bg-slate-50 px-4 py-4">
      <p className="text-sm text-slate-500">{label}</p>
      <p className="mt-2 text-lg font-semibold text-ink">{value}</p>
    </div>
  )
}
