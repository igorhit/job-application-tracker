export type ApplicationStatus =
  | 'Wishlist'
  | 'Applied'
  | 'Interview'
  | 'Challenge'
  | 'Offer'
  | 'Rejected'

export const APPLICATION_STATUS_LABELS: Record<ApplicationStatus, string> = {
  Wishlist: 'Wishlist',
  Applied: 'Aplicado',
  Interview: 'Entrevista',
  Challenge: 'Desafio',
  Offer: 'Oferta',
  Rejected: 'Rejeitado',
}

export const APPLICATION_STATUS_COLORS: Record<ApplicationStatus, string> = {
  Wishlist: 'bg-gray-100 text-gray-700',
  Applied: 'bg-blue-100 text-blue-700',
  Interview: 'bg-yellow-100 text-yellow-700',
  Challenge: 'bg-purple-100 text-purple-700',
  Offer: 'bg-green-100 text-green-700',
  Rejected: 'bg-red-100 text-red-700',
}

export interface User {
  userId: string
  email: string
  name: string
}

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  email: string
  name: string
}

export interface Company {
  id: string
  name: string
  website?: string
  notes?: string
  createdAt: string
  applicationCount: number
}

export interface JobApplication {
  id: string
  companyId: string
  companyName: string
  jobTitle: string
  status: number
  statusLabel: ApplicationStatus
  jobUrl?: string
  location?: string
  salaryExpectation?: number
  appliedAt: string
  nextActionAt?: string
  nextActionNote?: string
  noteCount: number
  createdAt: string
}

export interface Note {
  id: string
  content: string
  createdAt: string
}

export interface DashboardData {
  total: number
  byStatus: Record<string, number>
  upcomingActions: UpcomingAction[]
}

export interface UpcomingAction {
  applicationId: string
  jobTitle: string
  companyName: string
  nextActionAt: string
  nextActionNote?: string
}
