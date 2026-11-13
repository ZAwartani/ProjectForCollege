import { useState } from 'react'
import AccountRequestsPanel from '../../components/admin/AccountRequestsPanel'
import { ACCOUNT_TYPES } from '../../constants/accountTypes'
import useAuth from '../../hooks/useAuth'
import useLanguage from '../../hooks/useLanguage'
import styles from './AdminDashboardPage.module.css'

const TABS = [
  { kind: ACCOUNT_TYPES.RESTAURANT, labelKey: 'admin.tabs.restaurants' },
  { kind: ACCOUNT_TYPES.ORGANIZATION, labelKey: 'admin.tabs.organizations' },
]

// Admin area: review the registration requests of restaurants and charity organizations.
export default function AdminDashboardPage() {
  const { t } = useLanguage()
  const { email } = useAuth()
  const [kind, setKind] = useState(ACCOUNT_TYPES.RESTAURANT)

  return (
    <section>
      <h1>{t('admin.dashboard.title')}</h1>
      <p className={styles.subtitle}>{t('admin.dashboard.subtitle')}</p>
      <p className={styles.subtitle}>{t('dashboard.signedInAs', { email })}</p>

      <div className={styles.tabs} role="tablist">
        {TABS.map((tab) => (
          <button
            key={tab.kind}
            type="button"
            role="tab"
            aria-selected={kind === tab.kind}
            className={`${styles.tab} ${kind === tab.kind ? styles.active : ''}`}
            onClick={() => setKind(tab.kind)}
          >
            {t(tab.labelKey)}
          </button>
        ))}
      </div>

      {/* key = kind: switching tabs starts a fresh panel, so a list never shows the other kind's data */}
      <AccountRequestsPanel key={kind} kind={kind} />
    </section>
  )
}
