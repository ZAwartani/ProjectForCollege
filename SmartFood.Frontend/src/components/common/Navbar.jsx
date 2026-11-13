import { NavLink, Link } from 'react-router-dom'
import useAuth from '../../hooks/useAuth'
import useLanguage from '../../hooks/useLanguage'
import { HOME_PATH_BY_ACCOUNT_TYPE } from '../../constants/accountTypes'
import Button from '../ui/Button'
import LanguageToggle from './LanguageToggle'
import styles from './Navbar.module.css'

export default function Navbar() {
  const { t } = useLanguage()
  const { status, email, accountType, logout } = useAuth()
  const isLoggedIn = status === 'authenticated'

  return (
    <header className={styles.navbar}>
      <div className={`container ${styles.inner}`}>
        <Link to="/" className={styles.brand}>
          {t('app.name')}
        </Link>

        <nav className={styles.links} aria-label="Main">
          <NavLink
            to="/"
            end
            className={({ isActive }) => [styles.homeLink, isActive ? styles.active : ''].filter(Boolean).join(' ')}
          >
            {t('nav.home')}
          </NavLink>
          {isLoggedIn ? (
            <NavLink
              to={HOME_PATH_BY_ACCOUNT_TYPE[accountType]}
              className={({ isActive }) => (isActive ? styles.active : undefined)}
            >
              {t('nav.dashboard')}
            </NavLink>
          ) : null}
        </nav>

        <div className={styles.actions}>
          {isLoggedIn ? (
            <>
              <span className={styles.email} dir="ltr">
                {email}
              </span>
              <Button variant="ghost" size="sm" onClick={logout}>
                {t('nav.logout')}
              </Button>
            </>
          ) : status === 'unauthenticated' ? (
            <>
              <Link to="/login" className={styles.textLink}>
                {t('nav.login')}
              </Link>
              <Link to="/register" className={styles.primaryLink}>
                {t('nav.register')}
              </Link>
            </>
          ) : null}
          <LanguageToggle />
        </div>
      </div>
    </header>
  )
}
