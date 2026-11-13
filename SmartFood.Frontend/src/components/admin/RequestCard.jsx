import { useState } from 'react'
import useLanguage from '../../hooks/useLanguage'
import { buildFileUrl } from '../../utils/fileUrl'
import Button from '../ui/Button'
import styles from './RequestCard.module.css'

// One pending registration request with its approve / reject actions.
// busy: 'approve' | 'reject' | null (the action running for this card).
export default function RequestCard({ request, busy, disabled, onApprove, onReject }) {
  const { t } = useLanguage()
  const [confirmingReject, setConfirmingReject] = useState(false)
  const documentUrl = buildFileUrl(request.documentPath)

  return (
    <article className={styles.card}>
      <h3 className={styles.name}>{request.name}</h3>

      <dl className={styles.details}>
        <div className={styles.row}>
          <dt>{t('admin.request.owner')}</dt>
          <dd>{request.ownerName}</dd>
        </div>
        <div className={styles.row}>
          <dt>{t('admin.request.registrationNumber')}</dt>
          <dd>{request.registrationNumber}</dd>
        </div>
        <div className={styles.row}>
          <dt>{t('admin.request.address')}</dt>
          <dd>{request.address}</dd>
        </div>
        <div className={styles.row}>
          <dt>{t('admin.request.document')}</dt>
          <dd>
            {documentUrl ? (
              <a href={documentUrl} target="_blank" rel="noopener noreferrer">
                {t('admin.request.viewDocument')}
              </a>
            ) : (
              <span className={styles.missing}>{t('admin.request.noDocument')}</span>
            )}
          </dd>
        </div>
      </dl>

      {confirmingReject ? (
        <div className={styles.confirm} role="alertdialog" aria-label={t('admin.request.confirmReject')}>
          <p>{t('admin.request.confirmReject')}</p>
          <div className={styles.actions}>
            <Button
              variant="danger"
              size="sm"
              loading={busy === 'reject'}
              disabled={disabled}
              onClick={async () => {
                await onReject(request)
                // If the rejection failed the card is still here; close the question so it can be retried.
                setConfirmingReject(false)
              }}
            >
              {t('admin.request.confirmYes')}
            </Button>
            <Button variant="ghost" size="sm" disabled={disabled} onClick={() => setConfirmingReject(false)}>
              {t('admin.request.cancel')}
            </Button>
          </div>
        </div>
      ) : (
        <div className={styles.actions}>
          <Button size="sm" loading={busy === 'approve'} disabled={disabled} onClick={() => onApprove(request)}>
            {t('admin.request.approve')}
          </Button>
          <Button variant="secondary" size="sm" disabled={disabled} onClick={() => setConfirmingReject(true)}>
            {t('admin.request.reject')}
          </Button>
        </div>
      )}
    </article>
  )
}
