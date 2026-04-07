import { APPLICATION_STATUS_LABELS, type JobApplication } from './types'

export type StudyPromptMode = 'quick-study' | 'interview-prep' | 'review-plan'

export const STUDY_PROMPT_MODE_OPTIONS: Array<{ value: StudyPromptMode; label: string; description: string }> = [
  {
    value: 'quick-study',
    label: 'Estudo rápido',
    description: 'Foco em compreensão rápida, priorização e prática imediata.',
  },
  {
    value: 'interview-prep',
    label: 'Preparação para entrevista',
    description: 'Foco em perguntas prováveis, respostas e pontos de comunicação.',
  },
  {
    value: 'review-plan',
    label: 'Plano de revisão',
    description: 'Foco em organizar revisão curta por blocos de tempo.',
  },
]

function buildRequirementsSection(application: JobApplication) {
  return application.requirements.length > 0
    ? application.requirements
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map((requirement, index) => `${index + 1}. ${requirement.content}`)
      .join('\n')
    : 'Nenhum requisito foi listado explicitamente.'
}

function buildContextBlock(application: JobApplication) {
  return [
    `- Cargo: ${application.jobTitle}`,
    `- Empresa: ${application.companyName}`,
    `- Status atual da candidatura: ${APPLICATION_STATUS_LABELS[application.statusLabel]}`,
    `- Local: ${application.location ?? 'Não informado'}`,
    `- URL da vaga: ${application.jobUrl ?? 'Não informada'}`,
    `- Próxima ação: ${application.nextActionNote ?? 'Não definida'}`,
  ].join('\n')
}

function buildQuickStudyPrompt(application: JobApplication) {
  return [
    `Estou me preparando para uma vaga de "${application.jobTitle}" na empresa "${application.companyName}".`,
    '',
    'Contexto da vaga:',
    buildContextBlock(application),
    '',
    'Principais requisitos mapeados:',
    buildRequirementsSection(application),
    '',
    'Quero um estudo rápido e altamente prático para ficar funcional nessa vaga.',
    'Monte a resposta em português do Brasil com:',
    '1. resumo objetivo do que preciso dominar',
    '2. explicação curta de cada requisito listado',
    '3. ordem de prioridade do que estudar primeiro',
    '4. checklist prático de revisão para hoje',
    '5. perguntas técnicas e comportamentais prováveis',
    '6. mini plano de estudo de 2 horas e de 1 dia',
    '7. sugestões de exercícios rápidos ou mini projeto para fixação',
    '',
    'Evite teoria excessiva e foque no que mais aumenta minha chance de performar bem em entrevista e desafio técnico.',
  ].join('\n')
}

function buildInterviewPrepPrompt(application: JobApplication) {
  return [
    `Vou participar de um processo seletivo para a vaga "${application.jobTitle}" na empresa "${application.companyName}".`,
    '',
    'Contexto da vaga:',
    buildContextBlock(application),
    '',
    'Requisitos principais da vaga:',
    buildRequirementsSection(application),
    '',
    'Quero uma preparação objetiva para entrevista técnica e comportamental.',
    'Monte a resposta em português do Brasil com:',
    '1. o que o recrutador e o entrevistador provavelmente querem ouvir para cada requisito',
    '2. perguntas técnicas prováveis com respostas curtas e didáticas',
    '3. perguntas comportamentais prováveis com exemplos de estrutura de resposta',
    '4. armadilhas comuns e erros de comunicação que devo evitar',
    '5. pontos de experiência prática que eu devo destacar mesmo com pouca senioridade',
    '6. um roteiro de revisão de 30 minutos antes da entrevista',
    '',
    'Seja direto, específico e voltado para performance em entrevista.',
  ].join('\n')
}

function buildReviewPlanPrompt(application: JobApplication) {
  return [
    `Preciso montar um plano enxuto de revisão para a vaga "${application.jobTitle}" da empresa "${application.companyName}".`,
    '',
    'Contexto da vaga:',
    buildContextBlock(application),
    '',
    'Requisitos principais da vaga:',
    buildRequirementsSection(application),
    '',
    'Monte um plano de revisão em português do Brasil com:',
    '1. blocos de estudo de 25 a 45 minutos por prioridade',
    '2. o que revisar hoje em 1 hora, 2 horas e 1 dia',
    '3. materiais ou tipos de recurso mais adequados para cada bloco',
    '4. exercícios ou validações rápidas para saber se aprendi o suficiente',
    '5. checklist final de revisão antes de entrevista ou desafio técnico',
    '',
    'Quero um plano pragmático, sem excesso de teoria e com foco em retenção rápida.',
  ].join('\n')
}

export function buildStudyPrompt(application: JobApplication, mode: StudyPromptMode) {
  switch (mode) {
    case 'interview-prep':
      return buildInterviewPrepPrompt(application)
    case 'review-plan':
      return buildReviewPlanPrompt(application)
    default:
      return buildQuickStudyPrompt(application)
  }
}
