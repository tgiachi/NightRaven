import { getJson } from './http'

export type ServerStatus = {
  name: string
  online: boolean
  playersOnline: number
  uptime: string
  tickRate: number
}

export async function getServerStatus(): Promise<ServerStatus> {
  return getJson<ServerStatus>('/api/status')
}
