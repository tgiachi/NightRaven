import type { FormEvent } from 'react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { KeyRound, Shield, UserRound } from 'lucide-react'
import { Shell } from '../../shared/layouts/Shell'
import { NightRavenCrest } from '../player/NightRavenCrest'
import { PlayerFooter, PlayerPortalShell } from '../player/PlayerPortalShell'

type FakeLoginPageProps = {
  section: 'player' | 'admin'
  title: string
  description: string
  primaryLabel: string
  redirectTo: string
}

export function FakeLoginPage({
  section,
  title,
  description,
  primaryLabel,
  redirectTo,
}: FakeLoginPageProps) {
  const navigate = useNavigate()
  const [username, setUsername] = useState(section === 'admin' ? 'staff' : 'player')
  const [password, setPassword] = useState('nightraven')

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    navigate(redirectTo)
  }

  const Icon = section === 'admin' ? Shield : UserRound

  if (section === 'player') {
    return (
      <PlayerPortalShell>
        <section className="player-login-layout">
          <div className="player-login-copy ornate-panel">
            <NightRavenCrest />
            <p className="section-label">NightRaven Account</p>
            <h1>{title}</h1>
            <p>{description}</p>
            <div className="login-realm-strip">
              <div>
                <span>Realm Status</span>
                <strong className="status-online">Online</strong>
              </div>
              <div>
                <span>Players Online</span>
                <strong>362 / 1000</strong>
              </div>
              <div>
                <span>Season</span>
                <strong>Spring</strong>
              </div>
            </div>
          </div>

          <form className="player-login-panel ornate-panel" onSubmit={handleSubmit}>
            <div className="panel-title">
              <UserRound aria-hidden="true" />
              <h2>Account Access</h2>
            </div>
            <p>Temporary login. Any value opens the player dashboard.</p>

            <label className="field fantasy-field">
              <span>Account name</span>
              <input
                value={username}
                autoComplete="username"
                onChange={(event) => setUsername(event.target.value)}
              />
            </label>

            <label className="field fantasy-field">
              <span>Password</span>
              <input
                type="password"
                value={password}
                autoComplete="current-password"
                onChange={(event) => setPassword(event.target.value)}
              />
            </label>

            <button className="gold-button login-submit" type="submit">
              <KeyRound aria-hidden="true" />
              {primaryLabel}
            </button>
          </form>
        </section>
        <PlayerFooter />
      </PlayerPortalShell>
    )
  }

  return (
    <Shell>
      <section className="login-screen">
        <div className="login-copy">
          <p className="section-label">{section}</p>
          <h1>{title}</h1>
          <p>{description}</p>
        </div>

        <form className="login-panel" onSubmit={handleSubmit}>
          <div className="login-panel-header">
            <div className="status-card-icon">
              <Icon aria-hidden="true" />
            </div>
            <div>
              <h2>Accesso temporaneo</h2>
              <p>Qualsiasi valore porta alla dashboard.</p>
            </div>
          </div>

          <label className="field">
            <span>Username</span>
            <input
              value={username}
              autoComplete="username"
              onChange={(event) => setUsername(event.target.value)}
            />
          </label>

          <label className="field">
            <span>Password</span>
            <input
              type="password"
              value={password}
              autoComplete="current-password"
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>

          <button className="primary-button" type="submit">
            <KeyRound aria-hidden="true" />
            {primaryLabel}
          </button>
        </form>
      </section>
    </Shell>
  )
}
