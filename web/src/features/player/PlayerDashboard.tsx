import { Download, ScrollText, UserRound } from 'lucide-react'
import { Shell } from '../../shared/layouts/Shell'
import { ServerStatusPanel } from '../../shared/components/ServerStatusPanel'
import { StatusCard } from '../../shared/components/StatusCard'

export function PlayerDashboard() {
  return (
    <Shell>
      <section className="page-heading">
        <div>
          <p className="section-label">Player</p>
          <h1>Area giocatori</h1>
        </div>
        <p>
          Base per account, personaggi, stato shard e download client. Per ora
          tiene separato cio che serve ai giocatori da cio che serve allo staff.
        </p>
      </section>

      <div className="dashboard-grid">
        <ServerStatusPanel />
        <StatusCard
          icon={<UserRound aria-hidden="true" />}
          label="Account"
          value="Login"
          detail="Pronto per /api/me e cookie HttpOnly"
        />
        <StatusCard
          icon={<ScrollText aria-hidden="true" />}
          label="Personaggi"
          value="0"
          detail="Da collegare alla futura API account"
        />
        <StatusCard
          icon={<Download aria-hidden="true" />}
          label="Client"
          value="Setup"
          detail="Spazio per download e configurazione"
        />
      </div>
    </Shell>
  )
}
