'use client'

import { useEffect, useState } from 'react'
import { companies as companiesApi, ApiError } from '@/lib/api'
import type { Company } from '@/lib/types'
import { PlusIcon, PencilIcon, TrashIcon, BuildingOfficeIcon, GlobeAltIcon } from '@heroicons/react/24/outline'

interface CompanyFormData {
  name: string
  website: string
  notes: string
}

const EMPTY_FORM: CompanyFormData = { name: '', website: '', notes: '' }

export default function CompaniesPage() {
  const [items, setItems] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<Company | null>(null)
  const [form, setForm] = useState<CompanyFormData>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [deleting, setDeleting] = useState<string | null>(null)

  async function load() {
    try {
      const data = await companiesApi.list()
      setItems(data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  function openNew() {
    setEditing(null)
    setForm(EMPTY_FORM)
    setError('')
    setShowForm(true)
  }

  function openEdit(company: Company) {
    setEditing(company)
    setForm({ name: company.name, website: company.website ?? '', notes: company.notes ?? '' })
    setError('')
    setShowForm(true)
  }

  function closeForm() {
    setShowForm(false)
    setEditing(null)
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      const payload = {
        name: form.name,
        website: form.website || undefined,
        notes: form.notes || undefined,
      }
      if (editing) {
        await companiesApi.update(editing.id, payload)
      } else {
        await companiesApi.create(payload)
      }
      closeForm()
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao salvar')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(id: string) {
    if (!confirm('Remover empresa? As candidaturas vinculadas serão afetadas.')) return
    setDeleting(id)
    try {
      await companiesApi.delete(id)
      setItems(prev => prev.filter(c => c.id !== id))
    } finally {
      setDeleting(null)
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-gray-900">Empresas</h1>
          <p className="text-sm text-gray-500">{items.length} empresa{items.length !== 1 ? 's' : ''} cadastrada{items.length !== 1 ? 's' : ''}</p>
        </div>
        <button
          onClick={openNew}
          className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-700 transition-colors"
        >
          <PlusIcon className="h-4 w-4" />
          Nova empresa
        </button>
      </div>

      {loading ? (
        <div className="space-y-3 animate-pulse">
          {[1, 2, 3].map(i => <div key={i} className="h-20 rounded-xl bg-gray-200" />)}
        </div>
      ) : items.length === 0 ? (
        <div className="rounded-xl border-2 border-dashed border-gray-200 p-10 text-center">
          <BuildingOfficeIcon className="mx-auto mb-2 h-8 w-8 text-gray-300" />
          <p className="text-sm text-gray-500">Nenhuma empresa cadastrada.</p>
          <button onClick={openNew} className="mt-2 text-sm text-blue-600 hover:underline">
            Adicionar empresa
          </button>
        </div>
      ) : (
        <div className="space-y-3">
          {items.map(company => (
            <div key={company.id} className="flex items-start justify-between rounded-xl border border-gray-200 bg-white p-4">
              <div className="min-w-0 flex-1">
                <p className="font-medium text-gray-900">{company.name}</p>
                {company.website && (
                  <a
                    href={company.website}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="mt-0.5 flex items-center gap-1 text-xs text-blue-600 hover:underline"
                  >
                    <GlobeAltIcon className="h-3 w-3" />
                    {company.website}
                  </a>
                )}
                {company.notes && (
                  <p className="mt-1 text-xs text-gray-500 line-clamp-2">{company.notes}</p>
                )}
                <p className="mt-1 text-xs text-gray-400">
                  {company.applicationCount} candidatura{company.applicationCount !== 1 ? 's' : ''}
                </p>
              </div>
              <div className="ml-4 flex gap-1">
                <button
                  onClick={() => openEdit(company)}
                  className="rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-700 transition-colors"
                >
                  <PencilIcon className="h-4 w-4" />
                </button>
                <button
                  onClick={() => handleDelete(company.id)}
                  disabled={deleting === company.id}
                  className="rounded-lg p-2 text-gray-400 hover:bg-red-50 hover:text-red-600 disabled:opacity-50 transition-colors"
                >
                  <TrashIcon className="h-4 w-4" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal de formulário */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
            <h2 className="mb-4 text-lg font-semibold text-gray-900">
              {editing ? 'Editar empresa' : 'Nova empresa'}
            </h2>
            <form onSubmit={handleSave} className="space-y-4">
              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">Nome *</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                  required
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">Site</label>
                <input
                  type="url"
                  value={form.website}
                  onChange={e => setForm(f => ({ ...f, website: e.target.value }))}
                  placeholder="https://"
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">Observações</label>
                <textarea
                  value={form.notes}
                  onChange={e => setForm(f => ({ ...f, notes: e.target.value }))}
                  rows={3}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 resize-none"
                />
              </div>
              {error && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}
              <div className="flex gap-2 pt-2">
                <button
                  type="button"
                  onClick={closeForm}
                  className="flex-1 rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="flex-1 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
                >
                  {saving ? 'Salvando...' : 'Salvar'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
