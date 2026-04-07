import type { JobApplication } from './types'
import { APPLICATION_STATUS_LABELS } from './types'

function escapeCsvValue(value: string | number) {
  return `"${String(value).replaceAll('"', '""')}"`
}

function formatDate(value?: string) {
  if (!value) return ''
  return new Intl.DateTimeFormat('pt-BR').format(new Date(value))
}

function formatCurrency(value?: number) {
  if (value == null) return ''
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value)
}

export function downloadApplicationsCsv(applications: JobApplication[]) {
  const headers = [
    'Cargo',
    'Empresa',
    'Status',
    'Principais requisitos',
    'Local',
    'URL da vaga',
    'Pretensão salarial',
    'Data da candidatura',
    'Próxima ação',
    'O que fazer',
    'Quantidade de notas',
    'Criado em',
  ]

  const rows = applications.map(application => [
    application.jobTitle,
    application.companyName,
    APPLICATION_STATUS_LABELS[application.statusLabel],
    application.requirements.map(requirement => requirement.content).join(' | '),
    application.location ?? '',
    application.jobUrl ?? '',
    formatCurrency(application.salaryExpectation),
    formatDate(application.appliedAt),
    formatDate(application.nextActionAt),
    application.nextActionNote ?? '',
    application.noteCount,
    formatDate(application.createdAt),
  ])

  const csvContent = `\uFEFF${[headers, ...rows]
    .map(row => row.map(value => escapeCsvValue(value)).join(';'))
    .join('\r\n')}`

  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')

  link.href = url
  link.download = `candidaturas-${new Date().toISOString().slice(0, 10)}.csv`
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}
