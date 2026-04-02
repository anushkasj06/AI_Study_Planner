export function LoadingSpinner({ label = 'Loading...' }: { label?: string }) {
  return (
    <div className="flex items-center gap-3 rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-600">
      <div className="h-4 w-4 animate-spin rounded-full border-2 border-brand border-t-transparent" />
      <span>{label}</span>
    </div>
  )
}
