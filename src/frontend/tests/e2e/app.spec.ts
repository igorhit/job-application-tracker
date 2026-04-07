import { readFile } from 'node:fs/promises'
import { expect, test, type APIRequestContext, type Page } from '@playwright/test'

const API_URL = process.env.PLAYWRIGHT_API_URL ?? 'http://localhost:5001'
const E2E_EMAIL = 'e2e@tracker.dev'
const E2E_PASSWORD = 'Password123!'
const E2E_NAME = 'E2E Test User'

async function getAccessToken(request: APIRequestContext) {
  const response = await request.post(`${API_URL}/auth/login`, {
    data: {
      email: E2E_EMAIL,
      password: E2E_PASSWORD,
    },
  })

  expect(response.ok()).toBeTruthy()
  const body = await response.json()
  return body.accessToken as string
}

async function cleanupPlaywrightArtifacts(request: APIRequestContext) {
  const accessToken = await getAccessToken(request)
  const headers = { Authorization: `Bearer ${accessToken}` }

  const applicationsResponse = await request.get(`${API_URL}/applications`, { headers })
  expect(applicationsResponse.ok()).toBeTruthy()
  const applications = await applicationsResponse.json()

  for (const application of applications) {
    if (typeof application.jobTitle === 'string' && application.jobTitle.startsWith('Playwright ')) {
      const response = await request.delete(`${API_URL}/applications/${application.id}`, { headers })
      expect(response.ok()).toBeTruthy()
    }
  }

  const companiesResponse = await request.get(`${API_URL}/companies`, { headers })
  expect(companiesResponse.ok()).toBeTruthy()
  const companies = await companiesResponse.json()

  for (const company of companies) {
    if (typeof company.name === 'string' && company.name.startsWith('Playwright ')) {
      const response = await request.delete(`${API_URL}/companies/${company.id}`, { headers })
      expect(response.ok()).toBeTruthy()
    }
  }
}

async function login(page: Page, email: string, password: string) {
  await page.goto('/login')
  await expect(page.getByRole('heading', { name: 'Entrar' })).toBeVisible()
  await page.getByLabel('E-mail').fill(email)
  await page.getByLabel('Senha').fill(password)
  await page.getByRole('button', { name: 'Entrar' }).click()
  await expect(page).toHaveURL(/\/dashboard/)
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()
}

test.describe('Application Tracker smoke flow', () => {
  test('logs in and navigates through the main authenticated pages', async ({ page }) => {
    await login(page, E2E_EMAIL, E2E_PASSWORD)

    await expect(page.getByText(E2E_NAME)).toBeVisible()
    await expect(page.getByRole('link', { name: 'Dashboard' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Candidaturas' })).toBeVisible()
    await expect(page.getByRole('link', { name: 'Empresas' })).toBeVisible()

    await page.getByRole('link', { name: 'Candidaturas' }).click()
    await expect(page).toHaveURL(/\/applications/)
    await expect(page.getByRole('heading', { name: 'Candidaturas' })).toBeVisible()
    await expect(page.getByPlaceholder('Buscar por cargo, empresa ou local...')).toBeVisible()

    await page.getByRole('link', { name: 'Empresas' }).click()
    await expect(page).toHaveURL(/\/companies/)
    await expect(page.getByRole('heading', { name: 'Empresas' })).toBeVisible()
    await expect(page.getByRole('button', { name: 'Nova empresa' })).toBeVisible()
  })

  test('creates, updates and removes a company, application and note via UI', async ({ page, request }) => {
    const suffix = `${Date.now()}-${test.info().workerIndex}`
    const companyName = `Playwright Company ${suffix}`
    const updatedCompanyName = `Playwright Company Updated ${suffix}`
    const jobTitle = `Playwright Role ${suffix}`
    const updatedJobTitle = `Playwright Senior Role ${suffix}`
    const companyCreateNotes = `Company created by Playwright ${suffix}.`
    const companyUpdateNotes = `Company updated by Playwright ${suffix}.`
    const noteContent = `Playwright note ${suffix}`
    const requirements = ['ASP.NET Core', 'Entity Framework', 'SQL', 'Testes automatizados']

    await cleanupPlaywrightArtifacts(request)
    await login(page, E2E_EMAIL, E2E_PASSWORD)

    await test.step('create a company', async () => {
      await page.goto('/companies')
      await page.getByRole('button', { name: 'Nova empresa' }).click()
      await expect(page.getByRole('heading', { name: 'Nova empresa' })).toBeVisible()
      await page.getByLabel('Nome *').fill(companyName)
      await page.getByLabel('Site').fill('https://example.com')
      await page.getByLabel('Observações').fill(companyCreateNotes)
      await page.getByRole('button', { name: 'Salvar' }).click()

      await expect(page.getByText(companyName)).toBeVisible()
      await expect(page.getByText(companyCreateNotes)).toBeVisible()
    })

    await test.step('edit the company', async () => {
      await page.getByRole('button', { name: `Editar empresa ${companyName}` }).click()
      await expect(page.getByRole('heading', { name: 'Editar empresa' })).toBeVisible()
      await page.getByLabel('Nome *').fill(updatedCompanyName)
      await page.getByLabel('Observações').fill(companyUpdateNotes)
      await page.getByRole('button', { name: 'Salvar' }).click()

      await expect(page.getByText(updatedCompanyName)).toBeVisible()
      await expect(page.getByText(companyUpdateNotes)).toBeVisible()
    })

    await test.step('create an application for the new company', async () => {
      await page.goto('/applications/new')
      await expect(page.getByRole('heading', { name: 'Nova candidatura' })).toBeVisible()
      await page.getByLabel('Empresa *').selectOption({ label: updatedCompanyName })
      await page.getByLabel('Cargo *').fill(jobTitle)
      await page.getByLabel('Status').selectOption({ label: 'Aplicado' })
      await page.getByLabel('Local').fill('Remote')
      await page.getByLabel('Pretensão salarial (R$)').fill('7000')
      await page.getByLabel('URL da vaga').fill('https://example.com/jobs/playwright')
      await page.getByLabel('Próxima ação em').fill('2026-04-15')
      await page.getByLabel('O que fazer').fill('Follow up with recruiter')
      await page.getByLabel('Principais requisitos').fill(requirements.join('\n'))
      await page.getByRole('button', { name: 'Salvar' }).click()

      await expect(page).toHaveURL(/\/applications\/.+/)
      await expect(page.getByRole('heading', { name: jobTitle })).toBeVisible()
      await expect(page.getByText(updatedCompanyName)).toBeVisible()
      await expect(page.getByText('Aplicado')).toBeVisible()
      await expect(page.getByText('ASP.NET Core')).toBeVisible()
      await expect(page.getByText('Entity Framework')).toBeVisible()
    })

    await test.step('generate the study prompt from the application details', async () => {
      await page.getByRole('button', { name: 'Gerar prompt' }).click()
      const promptTextarea = page.locator('textarea[readonly]')
      await expect(promptTextarea).toBeVisible()
      await expect(promptTextarea).toHaveValue(/Estou me preparando para uma vaga/)
      await expect(promptTextarea).toHaveValue(/ASP\.NET Core/)
      await expect(promptTextarea).toHaveValue(/Entity Framework/)

      await page.getByRole('button', { name: 'Preparação para entrevista' }).click()
      await expect(promptTextarea).toHaveValue(/preparação objetiva para entrevista técnica e comportamental/i)
      await expect(page.getByText('Modo selecionado: Preparação para entrevista')).toBeVisible()
      await expect(page.getByRole('button', { name: 'Gerar com IA' })).toBeDisabled()
      await expect(page.getByText(/Integração opcional de IA desabilitada/i)).toBeVisible()
    })

    await test.step('filter and sort applications from the list', async () => {
      await page.goto('/applications')
      await page.getByPlaceholder('Buscar por cargo, empresa ou local...').fill(jobTitle)
      await page.getByLabel('Filtrar por status').selectOption({ label: 'Aplicado' })
      await page.getByLabel('Filtrar por empresa').selectOption({ label: updatedCompanyName })
      await page.getByLabel('Ordenar candidaturas').selectOption('CompanyAsc')

      await expect(page.getByRole('link', { name: new RegExp(jobTitle) })).toBeVisible()
      await expect(page.getByPlaceholder('Buscar por cargo, empresa ou local...')).toHaveValue(jobTitle)
      await expect(page.getByLabel('Filtrar por status')).toHaveValue('Applied')
      await expect(page.getByLabel('Filtrar por empresa')).not.toHaveValue('')
      await expect(page.getByLabel('Ordenar candidaturas')).toHaveValue('CompanyAsc')
    })

    await test.step('export the filtered applications as CSV', async () => {
      const downloadPromise = page.waitForEvent('download')
      await page.getByRole('button', { name: 'Exportar CSV' }).click()
      const download = await downloadPromise

      expect(download.suggestedFilename()).toMatch(/^candidaturas-\d{4}-\d{2}-\d{2}\.csv$/)

      const filePath = await download.path()
      expect(filePath).not.toBeNull()

      const csvContent = await readFile(filePath!, 'utf-8')
      expect(csvContent).toContain('"Cargo";"Empresa";"Status"')
      expect(csvContent).toContain(jobTitle)
      expect(csvContent).toContain(updatedCompanyName)
      await expect(page.getByText('CSV exportado com 1 candidatura.')).toBeVisible()
    })

    await test.step('add and remove a note', async () => {
      await page.getByRole('link', { name: new RegExp(jobTitle) }).click()
      await expect(page.getByRole('heading', { name: jobTitle })).toBeVisible()
      await page.getByPlaceholder('Adicionar nota...').fill(noteContent)
      await page.getByRole('button', { name: 'Adicionar' }).click()
      await expect(page.getByText(noteContent)).toBeVisible()

      const noteCard = page.locator('div').filter({ hasText: noteContent }).first()
      await noteCard.getByRole('button', { name: 'Remover nota' }).click()
      await expect(page.getByText(noteContent)).not.toBeVisible()
    })

    await test.step('edit and remove the application', async () => {
      await page.getByRole('button', { name: 'Editar candidatura' }).click()
      await expect(page.getByRole('heading', { name: 'Editar candidatura' })).toBeVisible()
      await page.getByLabel('Cargo').fill(updatedJobTitle)
      await page.getByLabel('Status').selectOption({ label: 'Entrevista' })
      await page.getByLabel('Local').fill('Hybrid')
      await page.getByLabel('O que fazer').fill('Technical interview scheduled')
      await page.getByRole('button', { name: 'Salvar' }).click()

      await expect(page.getByRole('heading', { name: updatedJobTitle })).toBeVisible()
      await expect(page.getByText('Entrevista', { exact: true })).toBeVisible()

      await page.getByRole('button', { name: 'Remover candidatura' }).click()
      await expect(page.getByRole('heading', { name: 'Remover candidatura' })).toBeVisible()
      await page.getByRole('dialog').getByRole('button', { name: 'Remover', exact: true }).click()
      await expect(page).toHaveURL(/\/applications$/)
      await expect(page.getByText(updatedJobTitle)).not.toBeVisible()
      await expect(page.getByText('Candidatura removida com sucesso.')).toBeVisible()
    })

    await test.step('remove the company', async () => {
      await page.goto('/companies')
      await page.getByRole('button', { name: `Remover empresa ${updatedCompanyName}` }).click()
      await expect(page.getByRole('heading', { name: 'Remover empresa' })).toBeVisible()
      await page.getByRole('dialog').getByRole('button', { name: 'Remover', exact: true }).click()
      await expect(page.getByRole('button', { name: `Remover empresa ${updatedCompanyName}` })).toHaveCount(0)
      await expect(page.getByText('Empresa removida com sucesso.')).toBeVisible()
    })
  })
})
