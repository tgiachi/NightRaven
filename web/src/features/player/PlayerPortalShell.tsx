import type { PropsWithChildren } from 'react'
import { NavLink } from 'react-router-dom'
import {
  BookOpen,
  Download,
  Headphones,
  LogOut,
  Newspaper,
  Shield,
  UserRound,
} from 'lucide-react'
import { NightHeavenCrest } from './NightHeavenCrest'

export function PlayerPortalShell({ children }: PropsWithChildren) {
  return (
    <div className="player-portal-shell">
      <aside className="player-sidebar">
        <div className="player-brand">
          <NightHeavenCrest />
          <div className="player-brand-title">NightHeaven</div>
          <div className="player-brand-subtitle">Ultima Online Shard</div>
        </div>

        <nav className="player-nav" aria-label="Player portal navigation">
          <NavLink to="/player/dashboard">
            <UserRound aria-hidden="true" />
            Account
          </NavLink>
          <a href="#characters">
            <Shield aria-hidden="true" />
            Characters
          </a>
          <a href="#downloads">
            <Download aria-hidden="true" />
            Downloads
          </a>
          <a href="#news">
            <Newspaper aria-hidden="true" />
            News
          </a>
          <a href="#support">
            <Headphones aria-hidden="true" />
            Support
          </a>
        </nav>

        <div className="player-sidebar-art" />

        <div className="player-profile-strip">
          <NightHeavenCrest compact />
          <div>
            <strong>Arthorius</strong>
            <span>arthorius@nightheaven.net</span>
          </div>
        </div>

        <button className="player-logout" type="button">
          <LogOut aria-hidden="true" />
          Log out
        </button>
      </aside>

      <main className="player-content">{children}</main>
    </div>
  )
}

export function PlayerFooter() {
  return (
    <footer className="player-footer">
      <div>
        <span>Terms of Service</span>
        <span>Privacy Policy</span>
        <span>Code of Conduct</span>
      </div>
      <div className="footer-mark">
        <BookOpen aria-hidden="true" />
        <span>NH</span>
      </div>
      <span>&copy; 2026 NightHeaven. All rights reserved.</span>
    </footer>
  )
}
