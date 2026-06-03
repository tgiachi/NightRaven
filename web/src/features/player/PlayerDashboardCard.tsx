import { MoreHorizontal } from 'lucide-react'
import type { PlayerCharacter } from './playerPortalData'

type PlayerDashboardCardProps = {
  character: PlayerCharacter
}

const crestGlyphs: Record<PlayerCharacter['crest'], string> = {
  lion: 'L',
  star: 'A',
  stag: 'S',
}

export function PlayerDashboardCard({ character }: PlayerDashboardCardProps) {
  return (
    <article className="character-card">
      <div className={`character-banner character-banner-${character.crest}`}>
        {crestGlyphs[character.crest]}
      </div>

      <div className="character-card-heading">
        <h3>{character.name}</h3>
        <p>{character.title}</p>
      </div>

      <div className="character-portrait" aria-hidden="true">
        <div className="portrait-arch" />
        <div className="portrait-figure">
          <span className="figure-head" />
          <span className="figure-body" />
          <span className="figure-arm figure-arm-left" />
          <span className="figure-arm figure-arm-right" />
          <span className="figure-leg figure-leg-left" />
          <span className="figure-leg figure-leg-right" />
        </div>
        <div className="character-level">{character.level}</div>
      </div>

      <div className="character-meta">
        <strong>{character.shard}</strong>
        <span>Last login: {character.lastLogin}</span>
        <MoreHorizontal aria-label="Character actions" />
      </div>
    </article>
  )
}
