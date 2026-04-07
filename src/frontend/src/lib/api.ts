import type { AuthTokens, Company, DashboardData, JobApplication, Note, User } from './types'

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5001'

// Token em memória — não fica no localStorage para dificultar XSS
let _accessToken: string | null = null

export function setAccessToken(token: string) { _accessToken = token }
export function getAccessToken(): string | null { return _accessToken }
export function clearTokens() {
  _accessToken = null
  if (typeof window !== 'undefined') localStorage.removeItem('refreshToken')
}
export function getStoredRefreshToken(): string | null {
  if (typeof window === 'undefined') return null
  return localStorage.getItem('refreshToken')
}
export function storeRefreshToken(token: string) {
  if (typeof window !== 'undefined') localStorage.setItem('refreshToken', token)
}

export async function tryRefreshOnLoad(): Promise<boolean> {
  const refreshToken = getStoredRefreshToken()
  if (!refreshToken) return false
  return _tryRefresh(refreshToken)
}

async function _tryRefresh(refreshToken: string): Promise<boolean> {
  try {
    const res = await fetch(`${BASE_URL}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })
    if (!res.ok) return false
    const data: { accessToken: string; refreshToken: string } = await res.json()
    setAccessToken(data.accessToken)
    storeRefreshToken(data.refreshToken)
    return true
  } catch {
    return false
  }
}

export class ApiError extends Error {
  constructor(public status: number, public body: { errors?: string[]; message?: string }) {
    super(body.errors?.[0] ?? body.message ?? 'Erro desconhecido')
  }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }

  if (_accessToken) headers['Authorization'] = `Bearer ${_accessToken}`

  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers })

  if (res.status === 401) {
    const stored = getStoredRefreshToken()
    if (stored) {
      const refreshed = await _tryRefresh(stored)
      if (refreshed) {
        headers['Authorization'] = `Bearer ${_accessToken}`
        const retry = await fetch(`${BASE_URL}${path}`, { ...options, headers })
        if (!retry.ok) throw new ApiError(retry.status, await retry.json().catch(() => ({})))
        if (retry.status === 204) return undefined as T
        return retry.json()
      }
    }
    clearTokens()
    throw new ApiError(401, { message: 'Sessão expirada' })
  }

  if (!res.ok) {
    const body = await res.json().catch(() => ({}))
    throw new ApiError(res.status, body)
  }

  if (res.status === 204) return undefined as T
  return res.json()
}

// Auth
export const auth = {
  register: (email: string, password: string, name: string) =>
    request<User>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password, name }),
    }),

  login: async (email: string, password: string): Promise<AuthTokens> => {
    const data = await request<AuthTokens>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
    setAccessToken(data.accessToken)
    storeRefreshToken(data.refreshToken)
    return data
  },

  logout: async () => {
    try { await request('/auth/logout', { method: 'POST' }) } catch { /* ignora */ }
    clearTokens()
  },
}

// Companies
export const companies = {
  list: () => request<Company[]>('/companies'),

  create: (data: { name: string; website?: string; notes?: string }) =>
    request<{ id: string; name: string }>('/companies', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: string, data: { name: string; website?: string; notes?: string }) =>
    request<void>(`/companies/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    request<void>(`/companies/${id}`, { method: 'DELETE' }),
}

// Applications
export const applications = {
  list: () => request<JobApplication[]>('/applications'),

  search: (q: string) =>
    request<JobApplication[]>(`/applications/search?q=${encodeURIComponent(q)}`),

  get: (id: string) => request<JobApplication>(`/applications/${id}`),

  create: (data: {
    companyId: string
    jobTitle: string
    status: number
    jobUrl?: string
    location?: string
    salaryExpectation?: number
    appliedAt: string
    nextActionAt?: string
    nextActionNote?: string
  }) =>
    request<{ id: string; jobTitle: string; companyName: string }>('/applications', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: string, data: {
    jobTitle: string
    status: number
    jobUrl?: string
    location?: string
    salaryExpectation?: number
    appliedAt: string
    nextActionAt?: string
    nextActionNote?: string
  }) =>
    request<void>(`/applications/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    request<void>(`/applications/${id}`, { method: 'DELETE' }),
}

// Notes
export const notes = {
  list: (applicationId: string) =>
    request<Note[]>(`/applications/${applicationId}/notes`),

  create: (applicationId: string, content: string) =>
    request<Note>(`/applications/${applicationId}/notes`, {
      method: 'POST',
      body: JSON.stringify({ content }),
    }),

  delete: (applicationId: string, noteId: string) =>
    request<void>(`/applications/${applicationId}/notes/${noteId}`, { method: 'DELETE' }),
}

// Dashboard
export const dashboard = {
  get: () => request<DashboardData>('/dashboard'),
}
