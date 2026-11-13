import { Navigate, Outlet, useLocation } from 'react-router-dom'
import Spinner from '../components/ui/Spinner'
import useAuth from '../hooks/useAuth'
import useLanguage from '../hooks/useLanguage'
import { HOME_PATH_BY_ACCOUNT_TYPE } from '../constants/accountTypes'

// Only lets a logged-in user of the given account type see the nested pages.
// Visitors are sent to loginPath (the admin area has its own login page).
export default function ProtectedRoute({ accountType, loginPath = '/login' }) {
  const { status, accountType: currentType } = useAuth()
  const { t } = useLanguage()
  const location = useLocation()

  if (status === 'loading') return <Spinner label={t('common.loading')} />

  if (status === 'unauthenticated') {
    return <Navigate to={loginPath} replace state={{ from: location }} />
  }

  if (currentType !== accountType) {
    return <Navigate to={HOME_PATH_BY_ACCOUNT_TYPE[currentType] ?? '/'} replace />
  }

  return <Outlet />
}
