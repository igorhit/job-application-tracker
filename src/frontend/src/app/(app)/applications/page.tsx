'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import FeedbackBanner from '@/components/FeedbackBanner'
import { usePathname, useSearchParams } from 'next/navigation'
import { ApiError, applications as applicationsApi, companies as companiesApi } from '@/lib/api'
import { downloadApplicationsCsv } from '@/lib/csv'
import type { ApplicationSortBy, ApplicationStatus, Company, JobApplication } from '@/lib/types'
import { APPLICATION_STATUS_COLORS, APPLICATION_STATUS_LABELS } from '@/lib/types'
import { PlusIcon, MagnifyingGlassIcon, BriefcaseIcon, XMarkIcon, ArrowDownTrayIcon } from '@heroicons/react/24/outline'

const STATUS_FILTER_OPTIONS: Array<{ value: string; label: string }> = [
  { value: '', label: 'Todos' },
  { value: 'Applied', label: 'Aplicado' },
  { value: 'Interview', label: 'Entrevista' },
  { value: 'Challenge', label: 'Desafio' },
  { value: 'Offer', label: 'Oferta' },
  { value: 'Wishlist', label: 'Wishlist' },
  { value: 'Rejected', label: 'Rejeitado' },
]

const SORT_OPTIONS: Array<{ value: ApplicationSortBy; label: string }> = [
  { value: 'AppliedAtDesc', label: 'Mais recentes' },
  { value: 'AppliedAtAsc', label: 'Mais antigas' },
  { value: 'NextActionAsc', label: 'Próxima ação' },
  { value: 'CompanyAsc', label: 'Empresa (A-Z)' },
  { value: 'StatusAsc', label: 'Status' },
  { value: 'CreatedAtDesc', label: 'Cadastro recente' },
]

export default function ApplicationsPage() {
  const pathname = usePathname()
  const searchParams = useSearchParams()
  const initialQuery = searchParams.get('q') ?? ''
  const initialStatus = searchParams.get('status') ?? ''
  const initialCompanyId = searchParams.get('companyId') ?? ''
  const initialSortBy = (searchParams.get('sortBy') as ApplicationSortBy | null) ?? 'AppliedAtDesc'
  const initialMessage = searchParams.get('message')

  const [items, setItems] = useState<JobApplication[]>([])
  const [companies, setCompanies] = useState<Company[]>([])
  const [companiesLoading, setCompaniesLoading] = useState(true)
  const [companiesError, setCompaniesError] = useState('')
  const [loading, setLoading] = useState(true)
  const [exporting, setExporting] = useState(false)
  const [error, setError] = useState('')
  const [successMessage, setSuccessMessage] = useState(
    initialMessage === 'application-deleted' ? 'Candidatura removida com sucesso.' : ''
  )
  const [searchQuery, setSearchQuery] = useState(initialQuery)
  const [statusFilter, setStatusFilter] = useState(initialStatus)
  const [companyFilter, setCompanyFilter] = useState(initialCompanyId)
  const [sortBy, setSortBy] = useState<ApplicationSortBy>(initialSortBy)

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = await applicationsApi.list({
        q: searchQuery.trim() || undefined,
        status: statusFilter ? statusFilter as ApplicationStatus : undefined,
        companyId: companyFilter || undefined,
        sortBy,
      })
      setItems(data)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Não foi possível carregar as candidaturas.')
    } finally {
      setLoading(false)
    }
  }, [companyFilter, searchQuery, sortBy, statusFilter])

  const loadCompanies = useCallback(async () => {
    setCompaniesLoading(true)
    setCompaniesError('')
    try {
      const data = await companiesApi.list()
      setCompanies(data)
    } catch (err) {
      setCompanies([])
      setCompaniesError(err instanceof ApiError ? err.message : 'Não foi possível carregar as empresas para o filtro.')
    } finally {
      setCompaniesLoading(false)
    }
  }, [])

  useEffect(() => {
    loadCompanies()
  }, [loadCompanies])

  useEffect(() => {
    const timer = setTimeout(load, searchQuery ? 300 : 0)
    return () => clearTimeout(timer)
  }, [load, searchQuery])

  useEffect(() => {
    const params = new URLSearchParams()
    const trimmedQuery = searchQuery.trim()

    if (trimmedQuery) params.set('q', trimmedQuery)
    if (statusFilter) params.set('status', statusFilter)
    if (companyFilter) params.set('companyId', companyFilter)
    if (sortBy !== 'AppliedAtDesc') params.set('sortBy', sortBy)

    const nextUrl = params.toString() ? `${pathname}?${params.toString()}` : pathname
    window.history.replaceState(null, '', nextUrl)
  }, [companyFilter, pathname, searchQuery, sortBy, statusFilter])

  useEffect(() => {
    if (!initialMessage) return

    const params = new URLSearchParams(searchParams.toString())
    params.delete('message')
    const nextUrl = params.toString() ? `${pathname}?${params.toString()}` : pathname
    window.history.replaceState(null, '', nextUrl)
  }, [initialMessage, pathname, searchParams])

  const inputCls = 'w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-100 dark:placeholder-gray-500'
  const hasActiveFilters = !!searchQuery.trim() || !!statusFilter || !!companyFilter || sortBy !== 'AppliedAtDesc'
  const activeFilterCount = [!!searchQuery.trim(), !!statusFilter, !!companyFilter, sortBy !== 'AppliedAtDesc'].filter(Boolean).length
  const selectedCompany = companies.find(company => company.id === companyFilter)

  function resetFilters() {
    setLoading(true)
    setSearchQuery('')
    setStatusFilter('')
    setCompanyFilter('')
    setSortBy('AppliedAtDesc')
  }

  async function handleExportCsv() {
    setExporting(true)
    setError('')

    try {
      const exportItems = await applicationsApi.list({
        q: searchQuery.trim() || undefined,
        status: statusFilter ? statusFilter as ApplicationStatus : undefined,
        companyId: companyFilter || undefined,
        sortBy,
      })

      if (exportItems.length === 0) {
        setError('Não há candidaturas para exportar com os filtros atuais.')
        return
      }

      downloadApplicationsCsv(exportItems)
      setSuccessMessage(`CSV exportado com ${exportItems.length} candidatura${exportItems.length !== 1 ? 's' : ''}.`)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Não foi possível exportar o CSV.')
    } finally {
      setExporting(false)
    }
  }

  function handleSearchChange(value: string) {
    setLoading(true)
    setSearchQuery(value)
  }

  function handleStatusFilterChange(value: string) {
    setLoading(true)
    setStatusFilter(value)
  }

  function handleCompanyFilterChange(value: string) {
    setLoading(true)
    setCompanyFilter(value)
  }

  function handleSortByChange(value: ApplicationSortBy) {
    setLoading(true)
    setSortBy(value)
  }

  return (
    <div className="space-y-5">
      {successMessage && <FeedbackBanner variant="success" message={successMessage} onClose={() => setSuccessMessage('')} />}
      {error && <FeedbackBanner variant="error" message={error} onClose={() => setError('')} />}
      {companiesError && <FeedbackBanner variant="info" message={companiesError} onClose={() => setCompaniesError('')} />}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">Candidaturas</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">{items.length} resultado{items.length !== 1 ? 's' : ''}</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={handleExportCsv}
            disabled={loading || exporting}
            className="flex items-center gap-1.5 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-200 dark:hover:bg-gray-800"
          >
            <ArrowDownTrayIcon className="h-4 w-4" />
            {exporting ? 'Exportando...' : 'Exportar CSV'}
          </button>
          <Link
            href="/applications/new"
            className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-500 transition-colors"
          >
            <PlusIcon className="h-4 w-4" />
            Nova candidatura
          </Link>
        </div>
      </div>

      {/* Filtros */}
      <div className="grid gap-3 lg:grid-cols-[minmax(0,2fr)_minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)]">
        <div className="relative flex-1">
          <label htmlFor="applications-search" className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">
            Buscar
          </label>
          <MagnifyingGlassIcon className="absolute left-3 top-9 h-4 w-4 text-gray-400 dark:text-gray-500" />
          <input
            id="applications-search"
            type="text"
            value={searchQuery}
            onChange={e => handleSearchChange(e.target.value)}
            placeholder="Buscar por cargo, empresa ou local..."
            className={`${inputCls} pl-9`}
          />
          {searchQuery && (
            <button
              type="button"
              onClick={() => handleSearchChange('')}
              aria-label="Limpar busca"
              className="absolute right-3 top-[34px] rounded-md p-1 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-800 dark:hover:text-gray-300"
            >
              <XMarkIcon className="h-4 w-4" />
            </button>
          )}
        </div>
        <div>
          <label htmlFor="applications-status-filter" className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">
            Status
          </label>
          <select
            id="applications-status-filter"
            value={statusFilter}
            onChange={e => handleStatusFilterChange(e.target.value)}
            className={inputCls}
            aria-label="Filtrar por status"
          >
            {STATUS_FILTER_OPTIONS.map(opt => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </select>
        </div>
        <div>
          <label htmlFor="applications-company-filter" className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">
            Empresa
          </label>
          <select
            id="applications-company-filter"
            value={companyFilter}
            onChange={e => handleCompanyFilterChange(e.target.value)}
            className={inputCls}
            aria-label="Filtrar por empresa"
            disabled={companiesLoading || companies.length === 0}
          >
            <option value="">{companiesLoading ? 'Carregando empresas...' : 'Todas as empresas'}</option>
            {companies.map(company => (
              <option key={company.id} value={company.id}>{company.name}</option>
            ))}
          </select>
        </div>
        <div>
          <label htmlFor="applications-sort" className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300">
            Ordenação
          </label>
          <select
            id="applications-sort"
            value={sortBy}
            onChange={e => handleSortByChange(e.target.value as ApplicationSortBy)}
            className={inputCls}
            aria-label="Ordenar candidaturas"
          >
            {SORT_OPTIONS.map(option => (
              <option key={option.value} value={option.value}>{option.label}</option>
            ))}
          </select>
        </div>
      </div>

      {hasActiveFilters && (
        <div className="space-y-3 rounded-xl border border-gray-800 bg-gray-900/60 px-4 py-3">
          <div className="flex items-center justify-between gap-4">
            <p className="text-sm text-gray-400">
              {activeFilterCount} filtro{activeFilterCount !== 1 ? 's' : ''} ativo{activeFilterCount !== 1 ? 's' : ''}.
            </p>
            <button
              type="button"
              onClick={resetFilters}
              className="text-sm font-medium text-blue-400 transition-colors hover:text-blue-300"
            >
              Limpar filtros
            </button>
          </div>
          <div className="flex flex-wrap gap-2">
            {!!searchQuery.trim() && (
              <span className="rounded-full bg-gray-800 px-3 py-1 text-xs text-gray-300">Busca: {searchQuery.trim()}</span>
            )}
            {!!statusFilter && (
              <span className="rounded-full bg-gray-800 px-3 py-1 text-xs text-gray-300">
                Status: {APPLICATION_STATUS_LABELS[statusFilter as ApplicationStatus]}
              </span>
            )}
            {!!selectedCompany && (
              <span className="rounded-full bg-gray-800 px-3 py-1 text-xs text-gray-300">Empresa: {selectedCompany.name}</span>
            )}
            {sortBy !== 'AppliedAtDesc' && (
              <span className="rounded-full bg-gray-800 px-3 py-1 text-xs text-gray-300">
                Ordenação: {SORT_OPTIONS.find(option => option.value === sortBy)?.label}
              </span>
            )}
          </div>
        </div>
      )}

      {loading ? (
        <div className="space-y-3 animate-pulse">
          {[1, 2, 3, 4].map(i => <div key={i} className="h-24 rounded-xl bg-gray-200 dark:bg-gray-800" />)}
        </div>
      ) : error ? (
        <div className="rounded-xl border border-red-900/60 bg-red-950/30 p-6">
          <p className="text-sm text-red-300">Falha ao carregar a lista com os filtros atuais.</p>
          <div className="mt-3 flex flex-wrap gap-3">
            <button
              type="button"
              onClick={load}
              className="text-sm font-medium text-red-200 underline-offset-4 hover:underline"
            >
              Tentar novamente
            </button>
            {hasActiveFilters && (
              <button
                type="button"
                onClick={resetFilters}
                className="text-sm font-medium text-red-200 underline-offset-4 hover:underline"
              >
                Limpar filtros
              </button>
            )}
            {companiesError && (
              <button
                type="button"
                onClick={loadCompanies}
                className="text-sm font-medium text-red-200 underline-offset-4 hover:underline"
              >
                Recarregar empresas
              </button>
            )}
          </div>
        </div>
      ) : items.length === 0 ? (
        <div className="rounded-xl border-2 border-dashed border-gray-200 p-10 text-center dark:border-gray-800">
          <BriefcaseIcon className="mx-auto mb-2 h-8 w-8 text-gray-300 dark:text-gray-700" />
          <p className="text-sm text-gray-500 dark:text-gray-400">
            {searchQuery || statusFilter || companyFilter ? 'Nenhuma candidatura encontrada.' : 'Nenhuma candidatura ainda.'}
          </p>
          {hasActiveFilters ? (
            <button type="button" onClick={resetFilters} className="mt-2 text-sm text-blue-500 hover:underline">
              Limpar filtros
            </button>
          ) : (
            <Link href="/applications/new" className="mt-2 inline-block text-sm text-blue-500 hover:underline">
              Adicionar candidatura
            </Link>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {items.map(app => (
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
