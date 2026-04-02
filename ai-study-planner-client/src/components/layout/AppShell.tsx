import { Outlet } from 'react-router-dom'
import { Navbar } from './Navbar'
import { Sidebar } from './Sidebar'

export function AppShell() {
  return (
    <div className="page-shell min-h-screen">
      <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
        <Sidebar />
        <div className="space-y-6">
          <Navbar />
          <Outlet />
        </div>
      </div>
    </div>
  )
}
