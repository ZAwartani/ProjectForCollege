import { Navigate, Outlet } from 'react-router-dom'
import Spinner from '../components/ui/Spinner'
import useAuth from '../hooks/useAuth'
import useLanguage from '../hooks/useLanguage'
import { HOME_PATH_BY_ACCOUNT_TYPE } from '../constants/accountTypes'

// Pages for visitors only (login, register...). A logged-in user is sent to their home page.
export default function GuestRoute() {
  const { status, accountType } = useAuth()
  const { t } = useLanguage()

  if (status === 'loading') return <Spinner label={t('common.loading')} />

  if (status === 'authenticated') {
    return <Navigate to={HOME_PATH_BY_ACCOUNT_TYPE[accountType] ?? '/'} replace />
  }

  return <Outlet />
}
