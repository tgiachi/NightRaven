import { useQuery } from '@tanstack/react-query'
import { ServerCrash, Signal } from 'lucide-react'
import { getServerStatus } from '../../api/status'
import { StatusCard } from './StatusCard'

export function ServerStatusPanel() {
  const { data, isError, isLoading } = useQuery({
    queryKey: ['server-status'],
    queryFn: getServerStatus,
    refetchInterval: 30_000,
  })

  if (isLoading) {
    return (
      <StatusCard
        icon={<Signal aria-hidden="true" />}
        label="Shard"
        value="Caricamento"
        detail="Lettura /api/status"
      />
    )
  }

  if (isError || !data) {
    return (
      <StatusCard
        icon={<ServerCrash aria-hidden="true" />}
        label="Shard"
        value="Offline"
        detail="/api/status non ancora disponibile"
      />
    )
  }

  return (
    <StatusCard
      icon={<Signal aria-hidden="true" />}
      label={data.name}
      value={data.online ? 'Online' : 'Offline'}
      detail={`${data.playersOnline} online - ${data.tickRate} tick/s`}
    />
  )
}
