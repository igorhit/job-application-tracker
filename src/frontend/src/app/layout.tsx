import type { Metadata } from 'next'
import { AuthProvider } from '@/contexts/AuthContext'
import './globals.css'

export const metadata: Metadata = {
  title: 'Application Tracker',
  description: 'Acompanhe suas candidaturas a vagas',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt-BR" className="h-full dark">
      <body className="h-full bg-gray-50 text-gray-900 antialiased dark:bg-gray-950 dark:text-gray-100">
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  )
}
