import { getJson } from './http'

export type UserRole = 'player' | 'admin'

export type CurrentUser = {
  id: string
  displayName: string
  roles: UserRole[]
}

export async function getCurrentUser(): Promise<CurrentUser | null> {
  try {
    return await getJson<CurrentUser>('/api/me')
  } catch {
    return null
  }
}
