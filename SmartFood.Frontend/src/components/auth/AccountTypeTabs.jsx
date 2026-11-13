import useLanguage from '../../hooks/useLanguage'
import { ACCOUNT_TYPES } from '../../constants/accountTypes'
import styles from './AccountTypeTabs.module.css'

const OPTIONS = [
  { value: ACCOUNT_TYPES.RESTAURANT, labelKey: 'auth.accountType.restaurant' },
  { value: ACCOUNT_TYPES.ORGANIZATION, labelKey: 'auth.accountType.organization' },
]

// Lets the visitor choose between a restaurant account and a charity organization account.
export default function AccountTypeTabs({ value, onChange, disabled = false }) {
  const { t } = useLanguage()

  return (
    <div className={styles.group} role="radiogroup" aria-label={t('auth.accountType.label')}>
      {OPTIONS.map((option) => {
        const selected = option.value === value
        return (
          <button
            key={option.value}
            type="button"
            role="radio"
            aria-checked={selected}
            disabled={disabled}
            className={`${styles.option} ${selected ? styles.selected : ''}`}
            onClick={() => onChange(option.value)}
          >
            {t(option.labelKey)}
          </button>
        )
      })}
    </div>
  )
}
