'use client'

import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { BriefcaseIcon, BuildingOfficeIcon, ChartBarIcon, ArrowRightOnRectangleIcon } from '@heroicons/react/24/outline'
import { useAuth } from '@/contexts/AuthContext'

const NAV_ITEMS = [
  { href: '/dashboard', label: 'Dashboard', Icon: ChartBarIcon },
  { href: '/applications', label: 'Candidaturas', Icon: BriefcaseIcon },
  { href: '/companies', label: 'Empresas', Icon: BuildingOfficeIcon },
]

export default function Navbar() {
  const { user, logout } = useAuth()
  const pathname = usePathname()
  const router = useRouter()

  async function handleLogout() {
    await logout()
    router.replace('/login')
  }

  return (
    <nav className="bg-white border-b border-gray-200">
      <div className="max-w-6xl mx-auto px-4 flex h-14 items-center justify-between">
        <div className="flex items-center gap-6">
          <span className="font-semibold text-blue-600 text-sm">AppTracker</span>
          <div className="flex gap-1">
            {NAV_ITEMS.map(({ href, label, Icon }) => {
              const active = pathname.startsWith(href)
              return (
                <Link
                  key={href}
                  href={href}
                  className={`flex items-center gap-1.5 px-3 py-1.5 rounded-md text-sm font-medium transition-colors ${
                    active
                      ? 'bg-blue-50 text-blue-700'
                      : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                  }`}
                >
                  <Icon className="h-4 w-4" />
                  {label}
                </Link>
              )
            })}
          </div>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-sm text-gray-500">{user?.name}</span>
          <button
            onClick={handleLogout}
            className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-900 transition-colors"
          >
            <ArrowRightOnRectangleIcon className="h-4 w-4" />
            Sair
          </button>
        </div>
      </div>
    </nav>
  )
}
