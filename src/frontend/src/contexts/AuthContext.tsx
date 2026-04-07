'use client'

import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { auth, getAccessToken, getStoredRefreshToken, tryRefreshOnLoad } from '@/lib/api'

interface AuthUser {
  email: string
  name: string
}

interface AuthContextValue {
  user: AuthUser | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string, name: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

function parseJwtPayload(token: string | null): { email: string; name: string } | null {
  if (!token) return null
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return { email: payload.email, name: payload.name }
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [loading, setLoading] = useState(true)

  // Ao carregar, tenta restaurar sessão com o refresh token armazenado no localStorage
  useEffect(() => {
    async function restore() {
      const hasRefresh = getStoredRefreshToken()
      if (!hasRefresh) { setLoading(false); return }

      const ok = await tryRefreshOnLoad()
      if (ok) {
        const payload = parseJwtPayload(getAccessToken())
        if (payload) setUser(payload)
      }
      setLoading(false)
    }
    restore()
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const data = await auth.login(email, password)
    setUser({ email: data.email, name: data.name })
  }, [])

  const register = useCallback(async (email: string, password: string, name: string) => {
    await auth.register(email, password, name)
    await login(email, password)
  }, [login])

  const logout = useCallback(async () => {
    await auth.logout()
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
