import { Activity, Database, Gauge, ShieldCheck } from 'lucide-react'
import { Shell } from '../../shared/layouts/Shell'
import { ServerStatusPanel } from '../../shared/components/ServerStatusPanel'
import { StatusCard } from '../../shared/components/StatusCard'

const adminQueues = [
  { label: 'Sessioni attive', value: '0', detail: 'In attesa endpoint admin' },
  { label: 'Packet errors', value: '0', detail: 'Parser metrics JSON da collegare' },
  { label: 'Tick queue', value: '0', detail: 'Event bus depth' },
]

export function AdminDashboard() {
  return (
    <Shell>
      <section className="page-heading">
        <div>
          <p className="section-label">Admin</p>
          <h1>Console operativa</h1>
        </div>
        <p>
          Superficie read-only iniziale per osservare shard, network, game loop e
          script senza introdurre azioni distruttive troppo presto.
        </p>
      </section>

      <div className="dashboard-grid">
        <ServerStatusPanel />
        <StatusCard
          icon={<ShieldCheck aria-hidden="true" />}
          label="Accesso"
          value="Staff"
          detail="Route pronta per role guard server-side"
        />
        <StatusCard
          icon={<Gauge aria-hidden="true" />}
          label="Metriche"
          value="JSON API"
          detail="Da derivare dai provider gia presenti"
        />
        <StatusCard
          icon={<Database aria-hidden="true" />}
          label="Scripts"
          value="Lua"
          detail="Modulo e file watcher gia lato server"
        />
      </div>

      <section className="panel">
        <div className="panel-header">
          <div>
            <p className="section-label">Monitor</p>
            <h2>Code e segnali runtime</h2>
          </div>
          <Activity aria-hidden="true" />
        </div>
        <div className="metric-list">
          {adminQueues.map((item) => (
            <div className="metric-row" key={item.label}>
              <span>{item.label}</span>
              <strong>{item.value}</strong>
              <small>{item.detail}</small>
            </div>
          ))}
        </div>
      </section>
    </Shell>
  )
}
