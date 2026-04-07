'use client'

import { CheckCircleIcon, ExclamationTriangleIcon, InformationCircleIcon, XMarkIcon } from '@heroicons/react/24/outline'

type FeedbackVariant = 'success' | 'error' | 'info'

interface FeedbackBannerProps {
  variant: FeedbackVariant
  message: string
  onClose?: () => void
}

const STYLES: Record<FeedbackVariant, { container: string; icon: typeof CheckCircleIcon }> = {
  success: {
    container: 'border border-emerald-900/60 bg-emerald-950/40 text-emerald-300',
    icon: CheckCircleIcon,
  },
  error: {
    container: 'border border-red-900/60 bg-red-950/40 text-red-300',
    icon: ExclamationTriangleIcon,
  },
  info: {
    container: 'border border-blue-900/60 bg-blue-950/40 text-blue-300',
    icon: InformationCircleIcon,
  },
}

export default function FeedbackBanner({ variant, message, onClose }: FeedbackBannerProps) {
  const { container, icon: Icon } = STYLES[variant]

  return (
    <div className={`flex items-start gap-3 rounded-xl px-4 py-3 text-sm ${container}`}>
      <Icon className="mt-0.5 h-5 w-5 shrink-0" />
      <p className="flex-1">{message}</p>
      {onClose && (
        <button
          type="button"
          onClick={onClose}
          aria-label="Fechar aviso"
          className="rounded-md p-1 transition-colors hover:bg-black/10"
        >
          <XMarkIcon className="h-4 w-4" />
        </button>
      )}
    </div>
  )
}
