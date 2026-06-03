import { NavLink } from 'react-router-dom'
import { Activity, Shield, UserRound } from 'lucide-react'
import type { PropsWithChildren } from 'react'

export function Shell({ children }: PropsWithChildren) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <NavLink to="/" className="brand" aria-label="NightHeaven home">
          <span className="brand-mark">NH</span>
          <span>NightHeaven</span>
        </NavLink>

        <nav className="main-nav" aria-label="Navigazione principale">
          <NavLink to="/player">
            <UserRound aria-hidden="true" />
            Player
          </NavLink>
          <NavLink to="/admin">
            <Shield aria-hidden="true" />
            Admin
          </NavLink>
        </nav>

        <div className="sidebar-status">
          <Activity aria-hidden="true" />
          <span>Backend proxy: localhost:5265</span>
        </div>
      </aside>

      <main className="content">{children}</main>
    </div>
  )
}
