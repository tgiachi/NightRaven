type NightHeavenCrestProps = {
  compact?: boolean
}

export function NightHeavenCrest({ compact = false }: NightHeavenCrestProps) {
  return (
    <div className={compact ? 'nh-crest nh-crest-compact' : 'nh-crest'} aria-hidden="true">
      <svg viewBox="0 0 120 120" role="img">
        <defs>
          <linearGradient id="nh-crest-fill" x1="0" x2="1" y1="0" y2="1">
            <stop offset="0%" stopColor="#2d2418" />
            <stop offset="48%" stopColor="#151312" />
            <stop offset="100%" stopColor="#3a2a13" />
          </linearGradient>
        </defs>
        <path className="nh-crest-wing" d="M16 48 43 35 60 7l17 28 27 13-18 12 8 31-25-10-9 22-9-22-25 10 8-31Z" />
        <path className="nh-crest-shield" d="M60 18 86 34v29c0 23-14 37-26 44-12-7-26-21-26-44V34Z" />
        <path className="nh-crest-line" d="M60 25v74M42 42h36M48 42v43M72 42v43" />
        <path className="nh-crest-sword" d="M60 5v104M51 33h18M55 109h10" />
      </svg>
    </div>
  )
}
