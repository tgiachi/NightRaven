export type ApiError = {
  status: number
  message: string
}

const defaultHeaders: HeadersInit = {
  Accept: 'application/json',
}

export async function getJson<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    credentials: 'include',
    headers: {
      ...defaultHeaders,
      ...init?.headers,
    },
  })

  if (!response.ok) {
    throw {
      status: response.status,
      message: response.statusText || 'Request failed',
    } satisfies ApiError
  }

  return response.json() as Promise<T>
}
