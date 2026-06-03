import type { FormEvent } from 'react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { KeyRound, Shield, UserRound } from 'lucide-react'
import { Shell } from '../../shared/layouts/Shell'

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
  const [password, setPassword] = useState('nightheaven')

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    navigate(redirectTo)
  }

  const Icon = section === 'admin' ? Shield : UserRound

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
