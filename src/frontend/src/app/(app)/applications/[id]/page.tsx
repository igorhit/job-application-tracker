'use client'

import { use, useCallback, useEffect, useState } from 'react'
import ConfirmDialog from '@/components/ConfirmDialog'
import FeedbackBanner from '@/components/FeedbackBanner'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { ai as aiApi, applications as applicationsApi, notes as notesApi, ApiError } from '@/lib/api'
import { parseRequirementsInput, stringifyRequirements } from '@/lib/requirements'
import { buildStudyPrompt, STUDY_PROMPT_MODE_OPTIONS, type StudyPromptMode } from '@/lib/studyPrompt'
import type { AiStatus, AiStudyAssistantResponse, ApplicationStatus, JobApplication, Note } from '@/lib/types'
import { APPLICATION_STATUS_COLORS, APPLICATION_STATUS_LABELS } from '@/lib/types'
import { PencilIcon, TrashIcon, PlusIcon, XMarkIcon, GlobeAltIcon, DocumentTextIcon, SparklesIcon, ClipboardDocumentIcon } from '@heroicons/react/24/outline'

const STATUS_OPTIONS = [
  { value: 0, label: 'Wishlist' },
  { value: 1, label: 'Aplicado' },
  { value: 2, label: 'Entrevista' },
  { value: 3, label: 'Desafio' },
  { value: 4, label: 'Oferta' },
  { value: 5, label: 'Rejeitado' },
]

const inputCls = 'w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-100 dark:placeholder-gray-500'
const labelCls = 'mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300'

export default function ApplicationDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()

  const [app, setApp] = useState<JobApplication | null>(null)
  const [notesList, setNotesList] = useState<Note[]>([])
  const [loading, setLoading] = useState(true)
  const [editing, setEditing] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [loadError, setLoadError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [noteError, setNoteError] = useState('')
  const [newNote, setNewNote] = useState('')
  const [addingNote, setAddingNote] = useState(false)
  const [deletingNote, setDeletingNote] = useState<string | null>(null)
  const [deletingApplication, setDeletingApplication] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [studyPromptVisible, setStudyPromptVisible] = useState(false)
  const [studyPromptMode, setStudyPromptMode] = useState<StudyPromptMode>('quick-study')
  const [aiStatus, setAiStatus] = useState<AiStatus | null>(null)
  const [loadingAiStatus, setLoadingAiStatus] = useState(true)
  const [generatingAi, setGeneratingAi] = useState(false)
  const [aiResponse, setAiResponse] = useState<AiStudyAssistantResponse | null>(null)

  const [form, setForm] = useState({
    jobTitle: '',
    status: 1,
    jobUrl: '',
    location: '',
    salaryExpectation: '',
    appliedAt: '',
    nextActionAt: '',
    nextActionNote: '',
    requirements: '',
  })

  const loadAll = useCallback(async () => {
    try {
      setLoadError('')
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
        requirements: stringifyRequirements(appData.requirements),
      })
    } catch (err) {
      setLoadError(err instanceof ApiError ? err.message : 'Não foi possível carregar a candidatura.')
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    loadAll()
  }, [loadAll])

  const loadAiStatus = useCallback(async () => {
    setLoadingAiStatus(true)
    try {
      setAiStatus(await aiApi.getStatus())
    } catch {
      setAiStatus({
        enabled: false,
        provider: 'OpenAI',
        model: 'gpt-5-mini',
        message: 'Não foi possível verificar a disponibilidade da integração opcional de IA agora.',
      })
    } finally {
      setLoadingAiStatus(false)
    }
  }, [])

  useEffect(() => {
    loadAiStatus()
  }, [loadAiStatus])

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
        requirements: parseRequirementsInput(form.requirements),
      })
      await loadAll()
      setEditing(false)
      setSuccessMessage('Candidatura atualizada com sucesso.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao salvar')
    } finally {
      setSaving(false)
    }
  }

  async function handleCopyStudyPrompt() {
    if (!app) return

    try {
      await navigator.clipboard.writeText(buildStudyPrompt(app, studyPromptMode))
      setSuccessMessage('Prompt de estudo copiado com sucesso.')
      setLoadError('')
    } catch {
      setLoadError('Não foi possível copiar o prompt automaticamente.')
    }
  }

  async function handleGenerateWithAi() {
    if (!app) return

    setGeneratingAi(true)
    setAiResponse(null)
    setLoadError('')

    try {
      const response = await applicationsApi.generateStudyAssistant(id, studyPromptMode)
      setAiResponse(response)
      setSuccessMessage('Conteúdo gerado com IA com sucesso.')
    } catch (err) {
      setLoadError(err instanceof ApiError ? err.message : 'Não foi possível gerar conteúdo com IA.')
    } finally {
      setGeneratingAi(false)
    }
  }

  async function handleDelete() {
    setDeletingApplication(true)
    try {
      await applicationsApi.delete(id)
      router.replace('/applications?message=application-deleted')
    } catch (err) {
      setLoadError(err instanceof ApiError ? err.message : 'Não foi possível remover a candidatura.')
    } finally {
      setDeletingApplication(false)
      setDeleteDialogOpen(false)
    }
  }

  async function handleAddNote(e: React.FormEvent) {
    e.preventDefault()
    if (!newNote.trim()) return
    setAddingNote(true)
    try {
      const note = await notesApi.create(id, newNote.trim())
      setNotesList(prev => [note, ...prev])
      setNewNote('')
      setSuccessMessage('Nota adicionada com sucesso.')
      setError('')
      setNoteError('')
    } catch (err) {
      setNoteError(err instanceof ApiError ? err.message : 'Não foi possível adicionar a nota.')
    } finally {
      setAddingNote(false)
    }
  }

  async function handleDeleteNote(noteId: string) {
    setDeletingNote(noteId)
    try {
      await notesApi.delete(id, noteId)
      setNotesList(prev => prev.filter(n => n.id !== noteId))
      setSuccessMessage('Nota removida com sucesso.')
      setNoteError('')
    } catch (err) {
      setNoteError(err instanceof ApiError ? err.message : 'Não foi possível remover a nota.')
    } finally {
      setDeletingNote(null)
    }
  }

  if (loading) return (
    <div className="space-y-4 animate-pulse max-w-2xl">
      <div className="h-8 w-64 rounded bg-gray-200 dark:bg-gray-800" />
      <div className="h-48 rounded-xl bg-gray-200 dark:bg-gray-800" />
      <div className="h-32 rounded-xl bg-gray-200 dark:bg-gray-800" />
    </div>
  )

  if (!app) {
    return (
      <div className="space-y-4">
        <FeedbackBanner variant="error" message={loadError || 'Candidatura não encontrada.'} />
        <button
          type="button"
          onClick={loadAll}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-500"
        >
          Tentar novamente
        </button>
      </div>
    )
  }

  const statusLabel = app.statusLabel as ApplicationStatus
  const studyPrompt = buildStudyPrompt(app, studyPromptMode)
  const selectedPromptMode = STUDY_PROMPT_MODE_OPTIONS.find(option => option.value === studyPromptMode)

  return (
    <div className="max-w-2xl space-y-6">
      {successMessage && <FeedbackBanner variant="success" message={successMessage} onClose={() => setSuccessMessage('')} />}
      {loadError && <FeedbackBanner variant="error" message={loadError} onClose={() => setLoadError('')} />}
      {/* Cabeçalho */}
      <div>
        <Link href="/applications" className="mb-3 inline-block text-sm text-gray-500 hover:text-gray-300 dark:text-gray-400">
          ← Candidaturas
        </Link>
        <div className="flex items-start justify-between gap-4">
          <div>
            <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">{app.jobTitle}</h1>
            <p className="text-sm text-gray-500 dark:text-gray-400">{app.companyName}</p>
          </div>
          <div className="flex gap-1 shrink-0">
            <button
              onClick={() => setEditing(e => !e)}
              aria-label="Editar candidatura"
              className="rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-700 dark:hover:bg-gray-800 dark:hover:text-gray-200 transition-colors"
            >
              <PencilIcon className="h-4 w-4" />
            </button>
            <button
              onClick={() => setDeleteDialogOpen(true)}
              aria-label="Remover candidatura"
              className="rounded-lg p-2 text-gray-400 hover:bg-red-950 hover:text-red-400 transition-colors"
            >
              <TrashIcon className="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Detalhes ou formulário de edição */}
      {editing ? (
        <form onSubmit={handleSave} className="space-y-4 rounded-xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-gray-900">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300">Editar candidatura</h2>
            <button type="button" onClick={() => setEditing(false)} aria-label="Fechar edição">
              <XMarkIcon className="h-4 w-4 text-gray-400 dark:text-gray-500" />
            </button>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <label htmlFor="application-detail-job-title" className={labelCls}>Cargo</label>
              <input id="application-detail-job-title" type="text" value={form.jobTitle} onChange={e => set('jobTitle', e.target.value)} required className={inputCls} />
            </div>
            <div>
              <label htmlFor="application-detail-status" className={labelCls}>Status</label>
              <select id="application-detail-status" value={form.status} onChange={e => set('status', Number(e.target.value))} className={inputCls}>
                {STATUS_OPTIONS.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
              </select>
            </div>
            <div>
              <label htmlFor="application-detail-applied-at" className={labelCls}>Data candidatura</label>
              <input id="application-detail-applied-at" type="date" value={form.appliedAt} onChange={e => set('appliedAt', e.target.value)} required className={inputCls} />
            </div>
            <div>
              <label htmlFor="application-detail-location" className={labelCls}>Local</label>
              <input id="application-detail-location" type="text" value={form.location} onChange={e => set('location', e.target.value)} className={inputCls} />
            </div>
            <div>
              <label htmlFor="application-detail-salary" className={labelCls}>Pretensão (R$)</label>
              <input id="application-detail-salary" type="number" value={form.salaryExpectation} onChange={e => set('salaryExpectation', e.target.value)} min="0" className={inputCls} />
            </div>
            <div className="sm:col-span-2">
              <label htmlFor="application-detail-job-url" className={labelCls}>URL da vaga</label>
              <input id="application-detail-job-url" type="url" value={form.jobUrl} onChange={e => set('jobUrl', e.target.value)} placeholder="https://" className={inputCls} />
            </div>
            <div>
              <label htmlFor="application-detail-next-action-at" className={labelCls}>Próxima ação em</label>
              <input id="application-detail-next-action-at" type="date" value={form.nextActionAt} onChange={e => set('nextActionAt', e.target.value)} className={inputCls} />
            </div>
            <div>
              <label htmlFor="application-detail-next-action-note" className={labelCls}>O que fazer</label>
              <input id="application-detail-next-action-note" type="text" value={form.nextActionNote} onChange={e => set('nextActionNote', e.target.value)} className={inputCls} />
            </div>
            <div className="sm:col-span-2">
              <label htmlFor="application-detail-requirements" className={labelCls}>Principais requisitos</label>
              <textarea
                id="application-detail-requirements"
                value={form.requirements}
                onChange={e => set('requirements', e.target.value)}
                rows={5}
                className={`${inputCls} resize-y`}
                placeholder={`Ex:\nASP.NET Core\nSQL\nTestes automatizados`}
              />
              <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                Um requisito por linha. Eles serão usados na geração do prompt de estudo.
              </p>
            </div>
          </div>
          {error && <p className="rounded-lg bg-red-950 px-3 py-2 text-sm text-red-400">{error}</p>}
          <div className="flex gap-2 pt-1">
            <button type="button" onClick={() => setEditing(false)}
              className="flex-1 rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 dark:border-gray-700 dark:text-gray-300 dark:hover:bg-gray-800 transition-colors">
              Cancelar
            </button>
            <button type="submit" disabled={saving}
              className="flex-1 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-500 disabled:opacity-50 transition-colors">
              {saving ? 'Salvando...' : 'Salvar'}
            </button>
          </div>
        </form>
      ) : (
        <div className="rounded-xl border border-gray-200 bg-white p-5 space-y-3 dark:border-gray-800 dark:bg-gray-900">
          <div className="flex flex-wrap gap-x-6 gap-y-2 text-sm">
            <div>
              <p className="text-xs text-gray-400 dark:text-gray-500">Status</p>
              <span className={`mt-0.5 inline-block rounded-full px-2 py-0.5 text-xs font-medium ${APPLICATION_STATUS_COLORS[statusLabel]}`}>
                {APPLICATION_STATUS_LABELS[statusLabel]}
              </span>
            </div>
            <div>
              <p className="text-xs text-gray-400 dark:text-gray-500">Candidatura em</p>
              <p className="text-sm text-gray-900 dark:text-gray-100">{new Date(app.appliedAt).toLocaleDateString('pt-BR')}</p>
            </div>
            {app.location && (
              <div>
                <p className="text-xs text-gray-400 dark:text-gray-500">Local</p>
                <p className="text-sm text-gray-900 dark:text-gray-100">{app.location}</p>
              </div>
            )}
            {app.salaryExpectation && (
              <div>
                <p className="text-xs text-gray-400 dark:text-gray-500">Pretensão</p>
                <p className="text-sm text-gray-900 dark:text-gray-100">
                  {app.salaryExpectation.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                </p>
              </div>
            )}
          </div>
          {app.jobUrl && (
            <a href={app.jobUrl} target="_blank" rel="noopener noreferrer"
              className="flex items-center gap-1 text-xs text-blue-500 hover:underline">
              <GlobeAltIcon className="h-3 w-3" />
              Ver vaga
            </a>
          )}
          {app.nextActionAt && (
            <div className="rounded-lg bg-blue-950 px-3 py-2">
              <p className="text-xs font-medium text-blue-400">
                Próxima ação: {new Date(app.nextActionAt).toLocaleDateString('pt-BR')}
              </p>
              {app.nextActionNote && <p className="text-xs text-blue-500 mt-0.5">{app.nextActionNote}</p>}
            </div>
          )}
          <div>
            <p className="text-xs text-gray-400 dark:text-gray-500">Principais requisitos</p>
            {app.requirements.length === 0 ? (
              <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                Nenhum requisito listado ainda.
              </p>
            ) : (
              <ul className="mt-2 space-y-1 text-sm text-gray-900 dark:text-gray-100">
                {app.requirements
                  .slice()
                  .sort((a, b) => a.displayOrder - b.displayOrder)
                  .map(requirement => (
                    <li key={requirement.id} className="flex gap-2">
                      <span className="text-blue-400">•</span>
                      <span>{requirement.content}</span>
                    </li>
                  ))}
              </ul>
            )}
          </div>
        </div>
      )}

      <div className="rounded-xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-gray-900">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h2 className="flex items-center gap-2 text-sm font-semibold text-gray-700 dark:text-gray-300">
              <SparklesIcon className="h-4 w-4" />
              Prompt de estudo com IA
            </h2>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              Gere um prompt pronto para colar em qualquer agente IA e estudar rápido os principais tópicos da vaga.
            </p>
          </div>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={() => setStudyPromptVisible(visible => !visible)}
              className="rounded-lg border border-gray-300 px-3 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50 dark:border-gray-700 dark:text-gray-200 dark:hover:bg-gray-800"
            >
              {studyPromptVisible ? 'Ocultar prompt' : 'Gerar prompt'}
            </button>
            <button
              type="button"
              onClick={handleCopyStudyPrompt}
              className="flex items-center gap-1 rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-500"
            >
              <ClipboardDocumentIcon className="h-4 w-4" />
              Copiar
            </button>
            <button
              type="button"
              onClick={handleGenerateWithAi}
              disabled={loadingAiStatus || !aiStatus?.enabled || generatingAi}
              className="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-emerald-500 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {generatingAi ? 'Gerando...' : 'Gerar com IA'}
            </button>
          </div>
        </div>

        <div className="mt-3 rounded-lg border border-dashed border-gray-300 bg-gray-50 px-3 py-2 dark:border-gray-700 dark:bg-gray-800/40">
          {loadingAiStatus ? (
            <p className="text-xs text-gray-500 dark:text-gray-400">
              Verificando disponibilidade da integração opcional de IA...
            </p>
          ) : aiStatus?.enabled ? (
            <p className="text-xs text-emerald-700 dark:text-emerald-400">
              {aiStatus.message}
            </p>
          ) : (
            <p className="text-xs text-gray-500 dark:text-gray-400">
              {aiStatus?.message ?? 'Integração opcional de IA indisponível no momento.'}
            </p>
          )}
        </div>

        <div className="mt-4 grid gap-3 md:grid-cols-3">
          {STUDY_PROMPT_MODE_OPTIONS.map(option => (
            <button
              key={option.value}
              type="button"
              onClick={() => {
                setStudyPromptMode(option.value)
                setAiResponse(null)
              }}
              aria-label={option.label}
              aria-pressed={studyPromptMode === option.value}
              className={`rounded-xl border p-3 text-left transition-colors ${
                studyPromptMode === option.value
                  ? 'border-blue-500 bg-blue-950/30'
                  : 'border-gray-200 bg-white hover:border-blue-400 dark:border-gray-800 dark:bg-gray-900 dark:hover:border-blue-500'
              }`}
            >
              <p className="text-sm font-medium text-gray-900 dark:text-gray-100">{option.label}</p>
              <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">{option.description}</p>
            </button>
          ))}
        </div>

        {studyPromptVisible && (
          <div className="mt-4 space-y-3">
            <div className="rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 dark:border-gray-800 dark:bg-gray-800/50">
              <p className="text-xs font-medium text-gray-700 dark:text-gray-300">
                Modo selecionado: {selectedPromptMode?.label}
              </p>
              <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                {selectedPromptMode?.description}
              </p>
            </div>
            <textarea
              readOnly
              value={studyPrompt}
              rows={16}
              className={`${inputCls} font-mono text-xs`}
            />
            <p className="text-xs text-gray-500 dark:text-gray-400">
              O prompt usa cargo, empresa, contexto da candidatura e os requisitos listados acima.
            </p>
          </div>
        )}

        {aiResponse && (
          <div className="mt-4 rounded-xl border border-emerald-200 bg-emerald-50/70 p-4 dark:border-emerald-900 dark:bg-emerald-950/20">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h3 className="text-sm font-semibold text-emerald-900 dark:text-emerald-300">
                  Resposta gerada com IA
                </h3>
                <p className="mt-1 text-xs text-emerald-700 dark:text-emerald-400">
                  {aiResponse.provider} · {aiResponse.model} · {new Date(aiResponse.generatedAtUtc).toLocaleString('pt-BR')}
                </p>
              </div>
            </div>
            <div className="mt-3 whitespace-pre-wrap text-sm text-gray-800 dark:text-gray-100">
              {aiResponse.content}
            </div>
          </div>
        )}
      </div>

      {/* Notas */}
      <div>
        <h2 className="mb-3 text-sm font-semibold text-gray-700 dark:text-gray-300">
          Notas ({notesList.length})
        </h2>
        {noteError && <div className="mb-3"><FeedbackBanner variant="error" message={noteError} onClose={() => setNoteError('')} /></div>}

        <form onSubmit={handleAddNote} className="mb-4 flex gap-2">
          <input
            type="text"
            value={newNote}
            onChange={e => setNewNote(e.target.value)}
            placeholder="Adicionar nota..."
            className={`flex-1 ${inputCls}`}
          />
          <button
            type="submit"
            disabled={!newNote.trim() || addingNote}
            className="flex items-center gap-1 rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-500 disabled:opacity-50 transition-colors"
          >
            <PlusIcon className="h-4 w-4" />
            Adicionar
          </button>
        </form>

        {notesList.length === 0 ? (
          <div className="rounded-xl border border-dashed border-gray-300 bg-gray-50 px-4 py-5 dark:border-gray-700 dark:bg-gray-800/40">
            <DocumentTextIcon className="mb-2 h-6 w-6 text-gray-400 dark:text-gray-500" />
            <p className="text-sm text-gray-500 dark:text-gray-400">Nenhuma nota ainda.</p>
            <p className="mt-1 text-sm text-gray-400 dark:text-gray-500">
              Registre próximos passos, feedbacks de entrevista ou links úteis para centralizar o histórico da candidatura.
            </p>
          </div>
        ) : (
          <div className="space-y-2">
            {notesList.map(note => (
              <div key={note.id} className="flex items-start justify-between rounded-lg border border-gray-200 bg-white px-4 py-3 dark:border-gray-800 dark:bg-gray-900">
                <div className="min-w-0 flex-1">
                  <p className="text-sm text-gray-900 dark:text-gray-100 whitespace-pre-wrap">{note.content}</p>
                  <time className="mt-1 block text-xs text-gray-400 dark:text-gray-500">
                    {new Date(note.createdAt).toLocaleString('pt-BR')}
                  </time>
                </div>
                <button
                  onClick={() => handleDeleteNote(note.id)}
                  disabled={deletingNote === note.id}
                  aria-label="Remover nota"
                  className="ml-3 shrink-0 rounded-lg p-1.5 text-gray-300 hover:bg-red-950 hover:text-red-400 disabled:opacity-50 transition-colors"
                >
                  <TrashIcon className="h-3.5 w-3.5" />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      <ConfirmDialog
        open={deleteDialogOpen}
        title="Remover candidatura"
        description={`A candidatura "${app.jobTitle}" será removida permanentemente.`}
        confirmLabel="Remover"
        onConfirm={handleDelete}
        onCancel={() => setDeleteDialogOpen(false)}
        loading={deletingApplication}
        tone="danger"
      />
    </div>
  )
}
