'use client'

import { useCallback, useEffect, useState } from 'react'
import Link from 'next/link'
import FeedbackBanner from '@/components/FeedbackBanner'
import { ApiError, dashboard } from '@/lib/api'
import { APPLICATION_STATUS_COLORS, APPLICATION_STATUS_LABELS, type ApplicationStatus, type DashboardData } from '@/lib/types'
import { CalendarDaysIcon, SparklesIcon } from '@heroicons/react/24/outline'

export default function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const response = await dashboard.get()
      setData(response)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Não foi possível carregar o dashboard.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  if (loading) return <Skeleton />

  if (!data) {
    return (
      <div className="space-y-4">
        <FeedbackBanner variant="error" message={error || 'Não foi possível carregar o dashboard.'} />
        <button
          type="button"
          onClick={load}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-500"
        >
          Tentar novamente
        </button>
      </div>
    )
  }

  const statusOrder: ApplicationStatus[] = ['Applied', 'Interview', 'Challenge', 'Offer', 'Wishlist', 'Rejected']

  return (
    <div className="space-y-6">
      {error && <FeedbackBanner variant="info" message={error} onClose={() => setError('')} />}
      <div>
        <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">Dashboard</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400">Visão geral das suas candidaturas</p>
      </div>

      {/* Totais por status */}
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
        {statusOrder.map(status => {
          const count = data.byStatus[status] ?? 0
          return (
            <Link
              key={status}
              href={`/applications?status=${status}`}
              className="rounded-xl border border-gray-200 bg-white p-4 hover:border-blue-500 transition-colors dark:border-gray-800 dark:bg-gray-900 dark:hover:border-blue-500"
            >
              <p className="text-2xl font-bold text-gray-900 dark:text-gray-100">{count}</p>
              <span className={`mt-1 inline-block rounded-full px-2 py-0.5 text-xs font-medium ${APPLICATION_STATUS_COLORS[status]}`}>
                {APPLICATION_STATUS_LABELS[status]}
              </span>
            </Link>
          )
        })}
      </div>

      {/* Total geral */}
      <div className="rounded-xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-gray-900">
        <p className="text-sm text-gray-500 dark:text-gray-400">Total de candidaturas</p>
        <p className="text-3xl font-bold text-gray-900 dark:text-gray-100">{data.total}</p>
      </div>

      {/* Próximas ações */}
      {data.upcomingActions.length > 0 && (
        <div>
          <h2 className="mb-3 flex items-center gap-2 text-sm font-semibold text-gray-700 dark:text-gray-300">
            <CalendarDaysIcon className="h-4 w-4" />
            Próximas ações
          </h2>
          <div className="space-y-2">
            {data.upcomingActions.map(action => (
              <Link
                key={action.applicationId}
                href={`/applications/${action.applicationId}`}
                className="flex items-start justify-between rounded-xl border border-gray-200 bg-white p-4 hover:border-blue-500 transition-colors dark:border-gray-800 dark:bg-gray-900 dark:hover:border-blue-500"
              >
                <div>
                  <p className="font-medium text-gray-900 text-sm dark:text-gray-100">{action.jobTitle}</p>
                  <p className="text-xs text-gray-500 dark:text-gray-400">{action.companyName}</p>
                  {action.nextActionNote && (
                    <p className="mt-1 text-xs text-gray-600 dark:text-gray-400">{action.nextActionNote}</p>
                  )}
                </div>
                <time className="text-xs text-gray-400 dark:text-gray-500 whitespace-nowrap ml-4">
                  {new Date(action.nextActionAt).toLocaleDateString('pt-BR')}
                </time>
              </Link>
            ))}
          </div>
        </div>
      )}

      {data.total > 0 && data.upcomingActions.length === 0 && (
        <div className="rounded-xl border border-blue-900/60 bg-blue-950/20 p-5">
          <div className="flex items-start gap-3">
            <SparklesIcon className="mt-0.5 h-5 w-5 text-blue-400" />
            <div>
              <p className="text-sm font-medium text-blue-200">Nenhuma próxima ação definida.</p>
              <p className="mt-1 text-sm text-blue-300/80">
                Atualize uma candidatura com data e nota de acompanhamento para manter este painel útil no dia a dia.
              </p>
              <Link href="/applications" className="mt-3 inline-block text-sm font-medium text-blue-300 hover:text-blue-200">
                Revisar candidaturas
              </Link>
            </div>
          </div>
        </div>
      )}

      {data.total === 0 && (
        <div className="rounded-xl border-2 border-dashed border-gray-200 p-10 text-center dark:border-gray-800">
          <p className="text-gray-500 dark:text-gray-400 text-sm">Nenhuma candidatura ainda.</p>
          <Link
            href="/applications/new"
            className="mt-2 inline-block text-sm text-blue-500 hover:underline"
          >
            Adicionar primeira candidatura
          </Link>
        </div>
      )}
    </div>
  )
}

function Skeleton() {
  return (
    <div className="space-y-6 animate-pulse">
      <div className="h-6 w-32 rounded bg-gray-200 dark:bg-gray-800" />
      <div className="grid grid-cols-3 gap-3 lg:grid-cols-6">
        {Array.from({ length: 6 }).map((_, i) => (
          <div key={i} className="h-20 rounded-xl bg-gray-200 dark:bg-gray-800" />
        ))}
      </div>
      <div className="h-24 rounded-xl bg-gray-200 dark:bg-gray-800" />
    </div>
  )
}
