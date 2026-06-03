import { Link } from 'react-router-dom'
import { Server, Shield } from 'lucide-react'
import { Shell } from '../../shared/layouts/Shell'

export function HomePage() {
  return (
    <Shell>
      <section className="home-split">
        <div className="home-copy">
          <h1>NightRaven web</h1>
          <p>
            Punto di ingresso unico per area giocatori e amministrazione shard.
            La struttura e pronta per collegare account, personaggi, metriche e
            controlli operativi dal backend ASP.NET Core.
          </p>
        </div>

        <div className="entry-actions" aria-label="Aree disponibili">
          <Link to="/player" className="entry-card">
            <Server aria-hidden="true" />
            <span>Area giocatori</span>
            <small>Status shard, account e personaggi.</small>
          </Link>
          <Link to="/admin" className="entry-card">
            <Shield aria-hidden="true" />
            <span>Admin</span>
            <small>Dashboard runtime e strumenti staff.</small>
          </Link>
        </div>
      </section>
    </Shell>
  )
}
