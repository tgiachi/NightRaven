import type { ReactNode } from 'react'

type StatusCardProps = {
  icon: ReactNode
  label: string
  value: string
  detail: string
}

export function StatusCard({ icon, label, value, detail }: StatusCardProps) {
  return (
    <article className="status-card">
      <div className="status-card-icon">{icon}</div>
      <div>
        <span>{label}</span>
        <strong>{value}</strong>
        <small>{detail}</small>
      </div>
    </article>
  )
}
