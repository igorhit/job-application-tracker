'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { applications as applicationsApi, companies as companiesApi, ApiError } from '@/lib/api'
import type { Company } from '@/lib/types'

const STATUS_OPTIONS = [
  { value: 0, label: 'Wishlist' },
  { value: 1, label: 'Aplicado' },
  { value: 2, label: 'Entrevista' },
  { value: 3, label: 'Desafio' },
  { value: 4, label: 'Oferta' },
  { value: 5, label: 'Rejeitado' },
]

export default function NewApplicationPage() {
  const router = useRouter()
  const [companiesList, setCompaniesList] = useState<Company[]>([])
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  const [form, setForm] = useState({
    companyId: '',
    jobTitle: '',
    status: 1,
    jobUrl: '',
    location: '',
    salaryExpectation: '',
    appliedAt: new Date().toISOString().split('T')[0],
    nextActionAt: '',
    nextActionNote: '',
  })

  useEffect(() => {
    companiesApi.list().then(data => {
      setCompaniesList(data)
      if (data.length > 0) setForm(f => ({ ...f, companyId: data[0].id }))
    })
  }, [])

  function set(field: string, value: string | number) {
    setForm(f => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!form.companyId) { setError('Selecione uma empresa'); return }
    setSaving(true)
    setError('')
    try {
      const result = await applicationsApi.create({
        companyId: form.companyId,
        jobTitle: form.jobTitle,
        status: form.status,
        jobUrl: form.jobUrl || undefined,
        location: form.location || undefined,
        salaryExpectation: form.salaryExpectation ? Number(form.salaryExpectation) : undefined,
        appliedAt: new Date(form.appliedAt).toISOString(),
        nextActionAt: form.nextActionAt ? new Date(form.nextActionAt).toISOString() : undefined,
        nextActionNote: form.nextActionNote || undefined,
      })
      router.replace(`/applications/${result.id}`)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao salvar')
      setSaving(false)
    }
  }

  return (
    <div className="max-w-2xl">
      <div className="mb-6">
        <button onClick={() => router.back()} className="mb-3 text-sm text-gray-500 hover:text-gray-700">
          ← Voltar
        </button>
        <h1 className="text-xl font-semibold text-gray-900">Nova candidatura</h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4 rounded-xl border border-gray-200 bg-white p-6">
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <label className="mb-1 block text-sm font-medium text-gray-700">Empresa *</label>
            {companiesList.length === 0 ? (
              <p className="text-sm text-gray-500">
                Nenhuma empresa cadastrada.{' '}
                <a href="/companies" className="text-blue-600 hover:underline">Cadastrar empresa</a>
              </p>
            ) : (
              <select
                value={form.companyId}
                onChange={e => set('companyId', e.target.value)}
                required
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                {companiesList.map(c => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            )}
          </div>

          <div className="sm:col-span-2">
            <label className="mb-1 block text-sm font-medium text-gray-700">Cargo *</label>
            <input
              type="text"
              value={form.jobTitle}
              onChange={e => set('jobTitle', e.target.value)}
              required
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Ex: Desenvolvedor Backend"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Status</label>
            <select
              value={form.status}
              onChange={e => set('status', Number(e.target.value))}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              {STATUS_OPTIONS.map(s => (
                <option key={s.value} value={s.value}>{s.label}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Data de candidatura *</label>
            <input
              type="date"
              value={form.appliedAt}
              onChange={e => set('appliedAt', e.target.value)}
              required
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Local</label>
            <input
              type="text"
              value={form.location}
              onChange={e => set('location', e.target.value)}
              placeholder="Ex: Remoto, São Paulo"
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Pretensão salarial (R$)</label>
            <input
              type="number"
              value={form.salaryExpectation}
              onChange={e => set('salaryExpectation', e.target.value)}
              min="0"
              step="100"
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div className="sm:col-span-2">
            <label className="mb-1 block text-sm font-medium text-gray-700">URL da vaga</label>
            <input
              type="url"
              value={form.jobUrl}
              onChange={e => set('jobUrl', e.target.value)}
              placeholder="https://"
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Próxima ação em</label>
            <input
              type="date"
              value={form.nextActionAt}
              onChange={e => set('nextActionAt', e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">O que fazer</label>
            <input
              type="text"
              value={form.nextActionNote}
              onChange={e => set('nextActionNote', e.target.value)}
              placeholder="Ex: Enviar portfólio"
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>
        </div>

        {error && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

        <div className="flex gap-2 pt-2">
          <button
            type="button"
            onClick={() => router.back()}
            className="flex-1 rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={saving || companiesList.length === 0}
            className="flex-1 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
          >
            {saving ? 'Salvando...' : 'Salvar'}
          </button>
        </div>
      </form>
    </div>
  )
}
