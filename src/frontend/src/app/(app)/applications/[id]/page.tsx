'use client'

import { use, useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { applications as applicationsApi, notes as notesApi, companies as companiesApi, ApiError } from '@/lib/api'
import type { ApplicationStatus, Company, JobApplication, Note } from '@/lib/types'
import { APPLICATION_STATUS_COLORS, APPLICATION_STATUS_LABELS } from '@/lib/types'
import { PencilIcon, TrashIcon, PlusIcon, XMarkIcon, GlobeAltIcon } from '@heroicons/react/24/outline'

const STATUS_OPTIONS = [
  { value: 0, label: 'Wishlist' },
  { value: 1, label: 'Aplicado' },
  { value: 2, label: 'Entrevista' },
  { value: 3, label: 'Desafio' },
  { value: 4, label: 'Oferta' },
  { value: 5, label: 'Rejeitado' },
]

export default function ApplicationDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [app, setApp] = useState<JobApplication | null>(null)
  const [notesList, setNotesList] = useState<Note[]>([])
  const [companiesList, setCompaniesList] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [editing, setEditing] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [newNote, setNewNote] = useState('')
  const [addingNote, setAddingNote] = useState(false)
  const [deletingNote, setDeletingNote] = useState<string | null>(null)

  const [form, setForm] = useState({
    jobTitle: '',
    status: 1,
    jobUrl: '',
    location: '',
    salaryExpectation: '',
    appliedAt: '',
    nextActionAt: '',
    nextActionNote: '',
  })

  async function loadAll() {
    try {
      const [appData, notesData] = await Promise.all([
        applicationsApi.get(id),
        notesApi.list(id),
      ])
      setApp(appData)
      setNotesList(notesData)
      setForm({
        jobTitle: appData.jobTitle,
        status: appData.status,
        jobUrl: appData.jobUrl ?? '',
        location: appData.location ?? '',
        salaryExpectation: appData.salaryExpectation?.toString() ?? '',
        appliedAt: appData.appliedAt.split('T')[0],
        nextActionAt: appData.nextActionAt?.split('T')[0] ?? '',
        nextActionNote: appData.nextActionNote ?? '',
      })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadAll()
    companiesApi.list().then(setCompaniesList)
  }, [id])

  function set(field: string, value: string | number) {
    setForm(f => ({ ...f, [field]: value }))
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      await applicationsApi.update(id, {
        jobTitle: form.jobTitle,
        status: form.status,
        jobUrl: form.jobUrl || undefined,
        location: form.location || undefined,
        salaryExpectation: form.salaryExpectation ? Number(form.salaryExpectation) : undefined,
        appliedAt: new Date(form.appliedAt).toISOString(),
        nextActionAt: form.nextActionAt ? new Date(form.nextActionAt).toISOString() : undefined,
        nextActionNote: form.nextActionNote || undefined,
      })
      await loadAll()
      setEditing(false)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao salvar')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    if (!confirm('Remover esta candidatura?')) return
    await applicationsApi.delete(id)
    router.replace('/applications')
  }

  async function handleAddNote(e: React.FormEvent) {
    e.preventDefault()
    if (!newNote.trim()) return
    setAddingNote(true)
    try {
      const note = await notesApi.create(id, newNote.trim())
      setNotesList(prev => [note, ...prev])
      setNewNote('')
    } finally {
      setAddingNote(false)
    }
  }

  async function handleDeleteNote(noteId: string) {
    setDeletingNote(noteId)
    try {
      await notesApi.delete(id, noteId)
      setNotesList(prev => prev.filter(n => n.id !== noteId))
    } finally {
      setDeletingNote(null)
    }
  }

  if (loading) return (
    <div className="space-y-4 animate-pulse max-w-2xl">
      <div className="h-8 w-64 rounded bg-gray-200" />
      <div className="h-48 rounded-xl bg-gray-200" />
      <div className="h-32 rounded-xl bg-gray-200" />
    </div>
  )

  if (!app) return <p className="text-gray-500">Candidatura não encontrada.</p>

  const statusLabel = app.statusLabel as ApplicationStatus

  return (
    <div className="max-w-2xl space-y-6">
      {/* Cabeçalho */}
      <div>
        <Link href="/applications" className="mb-3 inline-block text-sm text-gray-500 hover:text-gray-700">
          ← Candidaturas
        </Link>
        <div className="flex items-start justify-between gap-4">
          <div>
            <h1 className="text-xl font-semibold text-gray-900">{app.jobTitle}</h1>
            <p className="text-sm text-gray-500">{app.companyName}</p>
          </div>
          <div className="flex gap-1 shrink-0">
            <button
              onClick={() => setEditing(e => !e)}
              className="rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-700 transition-colors"
            >
              <PencilIcon className="h-4 w-4" />
            </button>
            <button
              onClick={handleDelete}
              className="rounded-lg p-2 text-gray-400 hover:bg-red-50 hover:text-red-600 transition-colors"
            >
              <TrashIcon className="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Detalhes ou formulário de edição */}
      {editing ? (
        <form onSubmit={handleSave} className="space-y-4 rounded-xl border border-gray-200 bg-white p-5">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-semibold text-gray-700">Editar candidatura</h2>
            <button type="button" onClick={() => setEditing(false)}>
              <XMarkIcon className="h-4 w-4 text-gray-400" />
            </button>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <label className="mb-1 block text-sm font-medium text-gray-700">Cargo</label>
              <input
                type="text"
                value={form.jobTitle}
                onChange={e => set('jobTitle', e.target.value)}
                required
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Status</label>
              <select
                value={form.status}
                onChange={e => set('status', Number(e.target.value))}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                {STATUS_OPTIONS.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
              </select>
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Data candidatura</label>
              <input type="date" value={form.appliedAt} onChange={e => set('appliedAt', e.target.value)} required
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Local</label>
              <input type="text" value={form.location} onChange={e => set('location', e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Pretensão (R$)</label>
              <input type="number" value={form.salaryExpectation} onChange={e => set('salaryExpectation', e.target.value)} min="0"
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />
            </div>
            <div className="sm:col-span-2">
              <label className="mb-1 block text-sm font-medium text-gray-700">URL da vaga</label>
              <input type="url" value={form.jobUrl} onChange={e => set('jobUrl', e.target.value)} placeholder="https://"
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Próxima ação em</label>
              <input type="date" value={form.nextActionAt} onChange={e => set('nextActionAt', e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">O que fazer</label>
              <input type="text" value={form.nextActionNote} onChange={e => set('nextActionNote', e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />
            </div>
          </div>
          {error && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}
          <div className="flex gap-2 pt-1">
            <button type="button" onClick={() => setEditing(false)}
              className="flex-1 rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors">
              Cancelar
            </button>
            <button type="submit" disabled={saving}
              className="flex-1 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 transition-colors">
              {saving ? 'Salvando...' : 'Salvar'}
            </button>
          </div>
        </form>
      ) : (
        <div className="rounded-xl border border-gray-200 bg-white p-5 space-y-3">
          <div className="flex flex-wrap gap-x-6 gap-y-2 text-sm">
            <div>
              <p className="text-xs text-gray-400">Status</p>
              <span className={`mt-0.5 inline-block rounded-full px-2 py-0.5 text-xs font-medium ${APPLICATION_STATUS_COLORS[statusLabel]}`}>
                {APPLICATION_STATUS_LABELS[statusLabel]}
              </span>
            </div>
            <div>
              <p className="text-xs text-gray-400">Candidatura em</p>
              <p className="text-sm text-gray-900">{new Date(app.appliedAt).toLocaleDateString('pt-BR')}</p>
            </div>
            {app.location && (
              <div>
                <p className="text-xs text-gray-400">Local</p>
                <p className="text-sm text-gray-900">{app.location}</p>
              </div>
            )}
            {app.salaryExpectation && (
              <div>
                <p className="text-xs text-gray-400">Pretensão</p>
                <p className="text-sm text-gray-900">
                  {app.salaryExpectation.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                </p>
              </div>
            )}
          </div>
          {app.jobUrl && (
            <a href={app.jobUrl} target="_blank" rel="noopener noreferrer"
              className="flex items-center gap-1 text-xs text-blue-600 hover:underline">
              <GlobeAltIcon className="h-3 w-3" />
              Ver vaga
            </a>
          )}
          {app.nextActionAt && (
            <div className="rounded-lg bg-blue-50 px-3 py-2">
              <p className="text-xs font-medium text-blue-700">
                Próxima ação: {new Date(app.nextActionAt).toLocaleDateString('pt-BR')}
              </p>
              {app.nextActionNote && <p className="text-xs text-blue-600 mt-0.5">{app.nextActionNote}</p>}
            </div>
          )}
        </div>
      )}

      {/* Notas */}
      <div>
        <h2 className="mb-3 text-sm font-semibold text-gray-700">
          Notas ({notesList.length})
        </h2>

        <form onSubmit={handleAddNote} className="mb-4 flex gap-2">
          <input
            type="text"
            value={newNote}
            onChange={e => setNewNote(e.target.value)}
            placeholder="Adicionar nota..."
            className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
          <button
            type="submit"
            disabled={!newNote.trim() || addingNote}
            className="flex items-center gap-1 rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
          >
            <PlusIcon className="h-4 w-4" />
            Adicionar
          </button>
        </form>

        {notesList.length === 0 ? (
          <p className="text-sm text-gray-400">Nenhuma nota ainda.</p>
        ) : (
          <div className="space-y-2">
            {notesList.map(note => (
              <div key={note.id} className="flex items-start justify-between rounded-lg border border-gray-200 bg-white px-4 py-3">
                <div className="min-w-0 flex-1">
                  <p className="text-sm text-gray-900 whitespace-pre-wrap">{note.content}</p>
                  <time className="mt-1 block text-xs text-gray-400">
                    {new Date(note.createdAt).toLocaleString('pt-BR')}
                  </time>
                </div>
                <button
                  onClick={() => handleDeleteNote(note.id)}
                  disabled={deletingNote === note.id}
                  className="ml-3 shrink-0 rounded-lg p-1.5 text-gray-300 hover:bg-red-50 hover:text-red-500 disabled:opacity-50 transition-colors"
                >
                  <TrashIcon className="h-3.5 w-3.5" />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
