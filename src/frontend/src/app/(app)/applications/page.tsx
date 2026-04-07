'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { useSearchParams } from 'next/navigation'
import { applications as applicationsApi } from '@/lib/api'
import type { ApplicationStatus, JobApplication } from '@/lib/types'
import { APPLICATION_STATUS_COLORS, APPLICATION_STATUS_LABELS } from '@/lib/types'
import { PlusIcon, MagnifyingGlassIcon, BriefcaseIcon } from '@heroicons/react/24/outline'

const STATUS_FILTER_OPTIONS: Array<{ value: string; label: string }> = [
  { value: '', label: 'Todos' },
  { value: 'Applied', label: 'Aplicado' },
  { value: 'Interview', label: 'Entrevista' },
  { value: 'Challenge', label: 'Desafio' },
  { value: 'Offer', label: 'Oferta' },
  { value: 'Wishlist', label: 'Wishlist' },
  { value: 'Rejected', label: 'Rejeitado' },
]

export default function ApplicationsPage() {
  const searchParams = useSearchParams()
  const initialStatus = searchParams.get('status') ?? ''

  const [items, setItems] = useState<JobApplication[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [statusFilter, setStatusFilter] = useState(initialStatus)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = searchQuery.trim()
        ? await applicationsApi.search(searchQuery.trim())
        : await applicationsApi.list()
      setItems(data)
    } finally {
      setLoading(false)
    }
  }, [searchQuery])

  useEffect(() => {
    const timer = setTimeout(load, searchQuery ? 300 : 0)
    return () => clearTimeout(timer)
  }, [load, searchQuery])

  const filtered = statusFilter
    ? items.filter(a => a.statusLabel === statusFilter)
    : items

  const inputCls = 'w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-100 dark:placeholder-gray-500'

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">Candidaturas</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">{filtered.length} resultado{filtered.length !== 1 ? 's' : ''}</p>
        </div>
        <Link
          href="/applications/new"
          className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-500 transition-colors"
        >
          <PlusIcon className="h-4 w-4" />
          Nova candidatura
        </Link>
      </div>

      {/* Filtros */}
      <div className="flex flex-col gap-3 sm:flex-row">
        <div className="relative flex-1">
          <MagnifyingGlassIcon className="absolute left-3 top-2.5 h-4 w-4 text-gray-400 dark:text-gray-500" />
          <input
            type="text"
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
            placeholder="Buscar por cargo, empresa ou local..."
            className={`${inputCls} pl-9`}
          />
        </div>
        <select
          value={statusFilter}
          onChange={e => setStatusFilter(e.target.value)}
          className={inputCls}
        >
          {STATUS_FILTER_OPTIONS.map(opt => (
            <option key={opt.value} value={opt.value}>{opt.label}</option>
          ))}
        </select>
      </div>

      {loading ? (
        <div className="space-y-3 animate-pulse">
          {[1, 2, 3, 4].map(i => <div key={i} className="h-24 rounded-xl bg-gray-200 dark:bg-gray-800" />)}
        </div>
      ) : filtered.length === 0 ? (
        <div className="rounded-xl border-2 border-dashed border-gray-200 p-10 text-center dark:border-gray-800">
          <BriefcaseIcon className="mx-auto mb-2 h-8 w-8 text-gray-300 dark:text-gray-700" />
          <p className="text-sm text-gray-500 dark:text-gray-400">
            {searchQuery || statusFilter ? 'Nenhuma candidatura encontrada.' : 'Nenhuma candidatura ainda.'}
          </p>
          {!searchQuery && !statusFilter && (
            <Link href="/applications/new" className="mt-2 inline-block text-sm text-blue-500 hover:underline">
              Adicionar candidatura
            </Link>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {filtered.map(app => (
            <Link
              key={app.id}
              href={`/applications/${app.id}`}
              className="flex items-start justify-between rounded-xl border border-gray-200 bg-white p-4 hover:border-blue-500 transition-colors dark:border-gray-800 dark:bg-gray-900 dark:hover:border-blue-500"
            >
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-2">
                  <p className="font-medium text-gray-900 dark:text-gray-100">{app.jobTitle}</p>
                  <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${APPLICATION_STATUS_COLORS[app.statusLabel as ApplicationStatus]}`}>
                    {APPLICATION_STATUS_LABELS[app.statusLabel as ApplicationStatus]}
                  </span>
                </div>
                <p className="mt-0.5 text-sm text-gray-500 dark:text-gray-400">{app.companyName}</p>
                {app.location && <p className="text-xs text-gray-400 dark:text-gray-500">{app.location}</p>}
                {app.nextActionAt && (
                  <p className="mt-1 text-xs text-blue-500">
                    Próxima ação: {new Date(app.nextActionAt).toLocaleDateString('pt-BR')}
                    {app.nextActionNote ? ` — ${app.nextActionNote}` : ''}
                  </p>
                )}
              </div>
              <div className="ml-4 text-right">
                <p className="text-xs text-gray-400 dark:text-gray-500">
                  {new Date(app.appliedAt).toLocaleDateString('pt-BR')}
                </p>
                {app.noteCount > 0 && (
                  <p className="text-xs text-gray-400 dark:text-gray-500">{app.noteCount} nota{app.noteCount !== 1 ? 's' : ''}</p>
                )}
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
