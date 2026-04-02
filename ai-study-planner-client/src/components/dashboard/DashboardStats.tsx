interface DashboardStatsProps {
  items: Array<{ label: string; value: string; tone?: string }>
}

export function DashboardStats({ items }: DashboardStatsProps) {
  return (
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      {items.map((item) => (
        <div key={item.label} className="glass-panel p-5">
          <p className="text-sm text-slate-500">{item.label}</p>
          <p className={`mt-3 text-3xl font-semibold ${item.tone ?? 'text-ink'}`}>{item.value}</p>
        </div>
      ))}
    </div>
  )
}
