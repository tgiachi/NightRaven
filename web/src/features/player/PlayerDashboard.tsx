import {
  Crown,
  Download,
  Heart,
  Newspaper,
  ScrollText,
  Settings,
  Swords,
  UserRound,
} from 'lucide-react'
import { NightRavenCrest } from './NightRavenCrest'
import { PlayerDashboardCard } from './PlayerDashboardCard'
import { PlayerFooter, PlayerPortalShell } from './PlayerPortalShell'
import { patchNotes, playerCharacters } from './playerPortalData'

export function PlayerDashboard() {
  return (
    <PlayerPortalShell>
      <div className="player-topbar ornate-panel">
        <div>
          <h1>Welcome back, <span>Arthorius</span></h1>
          <p>Member since May 12, 2024</p>
        </div>
        <div className="realm-kpis">
          <div>
            <span>Realm Status</span>
            <strong className="status-online">Online</strong>
          </div>
          <div>
            <span>Players Online</span>
            <strong>362 / 1000</strong>
          </div>
          <div>
            <span>Uptime</span>
            <strong>23h 47m</strong>
          </div>
        </div>
      </div>

      <div className="player-dashboard-grid">
        <section className="realm-status-card ornate-panel">
          <div className="panel-title">
            <Swords aria-hidden="true" />
            <h2>Realm Status</h2>
          </div>
          <div className="realm-copy">
            <p>
              NightRaven is <strong>online</strong> and accepting connections.
            </p>
            <span>All systems operational.</span>
          </div>
          <div className="realm-feature-row">
            <div>
              <Swords aria-hidden="true" />
              <span>Open PvP</span>
              <strong>Enabled</strong>
            </div>
            <div>
              <ScrollText aria-hidden="true" />
              <span>Trammel</span>
              <strong>Enabled</strong>
            </div>
            <div>
              <Crown aria-hidden="true" />
              <span>Siege Warfare</span>
              <strong>Active</strong>
            </div>
            <div>
              <Newspaper aria-hidden="true" />
              <span>Season</span>
              <strong>Spring</strong>
            </div>
          </div>
        </section>

        <section className="account-summary-card ornate-panel">
          <div className="panel-title">
            <UserRound aria-hidden="true" />
            <h2>Account Summary</h2>
          </div>
          <div className="account-summary-content">
            <div className="account-fields">
              <div>
                <span>Account Type</span>
                <strong><Crown aria-hidden="true" /> Premium</strong>
              </div>
              <div>
                <span>Membership</span>
                <strong className="status-online">Active</strong>
                <small>Renews on Jun 12, 2026</small>
              </div>
              <div>
                <span>Account Status</span>
                <strong className="status-online">In Good Standing</strong>
              </div>
            </div>
            <div className="account-medallion">
              <NightRavenCrest compact />
            </div>
          </div>
          <button className="metal-button" type="button">
            <Settings aria-hidden="true" />
            Manage Account
          </button>
        </section>

        <section className="characters-panel ornate-panel" id="characters">
          <div className="panel-title panel-title-row">
            <div>
              <UserRound aria-hidden="true" />
              <h2>Characters</h2>
            </div>
            <button className="metal-button" type="button">Manage Characters</button>
          </div>
          <div className="character-grid">
            {playerCharacters.map((character) => (
              <PlayerDashboardCard character={character} key={character.name} />
            ))}
            <article className="character-card character-card-empty">
              <div className="empty-character-mark">+</div>
              <p>Create New Character</p>
              <button className="gold-button" type="button">Create</button>
            </article>
          </div>
        </section>

        <aside className="player-side-column">
          <section className="download-card ornate-panel" id="downloads">
            <div className="panel-title">
              <Download aria-hidden="true" />
              <h2>Download Client</h2>
            </div>
            <div className="download-row">
              <Download aria-hidden="true" />
              <div>
                <strong>Full Client (Recommended)</strong>
                <span>UO Classic 7.0.98.13 + NightRaven</span>
                <small>1.2 GB</small>
              </div>
              <button className="gold-button" type="button">Download Full Client</button>
            </div>
            <div className="download-row">
              <Settings aria-hidden="true" />
              <div>
                <strong>Setup Guide</strong>
                <span>Step-by-step installation and connection</span>
              </div>
              <button className="metal-button" type="button">View Guide</button>
            </div>
          </section>

          <section className="news-card ornate-panel" id="news">
            <div className="panel-title panel-title-row">
              <div>
                <Newspaper aria-hidden="true" />
                <h2>News & Updates</h2>
              </div>
              <button className="link-button" type="button">View all</button>
            </div>
            <div className="news-list">
              {patchNotes.map((note) => (
                <article className="news-row" key={note.title}>
                  <ScrollText aria-hidden="true" />
                  <div>
                    <time>{note.date}</time>
                    <strong>{note.title}</strong>
                    <p>{note.summary}</p>
                  </div>
                </article>
              ))}
            </div>
          </section>

          <section className="community-card ornate-panel" id="support">
            <div className="panel-title">
              <Heart aria-hidden="true" />
              <h2>Community</h2>
            </div>
            <div className="community-links">
              <a href="/player/dashboard">Discord <span>Join our Discord</span></a>
              <a href="/player/dashboard">Forums <span>Discuss & Share</span></a>
              <a href="/player/dashboard">Rules <span>Server Rules</span></a>
              <a href="/player/dashboard">Wiki <span>Game Wiki</span></a>
            </div>
          </section>
        </aside>
      </div>

      <PlayerFooter />
    </PlayerPortalShell>
  )
}
