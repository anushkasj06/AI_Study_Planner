import { RadialBar, RadialBarChart, ResponsiveContainer } from 'recharts'

export function ProgressCard({
  title,
  value,
  subtitle
}: {
  title: string
  value: number
  subtitle: string
}) {
  return (
    <div className="glass-panel p-5">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-slate-500">{title}</p>
          <h3 className="mt-2 text-2xl font-semibold text-ink">{value}%</h3>
          <p className="mt-2 text-sm text-slate-500">{subtitle}</p>
        </div>
        <div className="h-28 w-28">
          <ResponsiveContainer>
            <RadialBarChart
              innerRadius="65%"
              outerRadius="100%"
              data={[{ name: title, value, fill: '#0f766e' }]}
              startAngle={90}
              endAngle={-270}
            >
              <RadialBar dataKey="value" cornerRadius={18} background />
            </RadialBarChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  )
}
