import { useCallback, useEffect, useState } from 'react'
import { approve, listAll, listPending, reject } from '../../api/adminService'
import { getErrorMessage } from '../../api/errors'
import useLanguage from '../../hooks/useLanguage'
import Alert from '../ui/Alert'
import Button from '../ui/Button'
import Spinner from '../ui/Spinner'
import RequestCard from './RequestCard'
import styles from './AccountRequestsPanel.module.css'

const VIEWS = { PENDING: 'pending', ALL: 'all' }

// Pending requests (with approve/reject) or the list of all accounts, for one account kind
// ("restaurant" or "organization").
export default function AccountRequestsPanel({ kind }) {
  const { t } = useLanguage()
  const [view, setView] = useState(VIEWS.PENDING)
  const [items, setItems] = useState([])
  const [status, setStatus] = useState('loading') // loading | ready | error
  const [loadError, setLoadError] = useState('')
  const [reloadCount, setReloadCount] = useState(0)
  const [action, setAction] = useState(null) // { id, type } while approve/reject runs
  const [message, setMessage] = useState(null) // { variant, text }

  // Load the list again whenever the kind, the view or the refresh counter changes.
  useEffect(() => {
    let cancelled = false
    const load = view === VIEWS.PENDING ? listPending : listAll
    load(kind)
      .then((result) => {
        if (cancelled) return
        setItems(result)
        setLoadError('')
        setStatus('ready')
      })
      .catch((error) => {
        if (cancelled) return
        setLoadError(getErrorMessage(error, t))
        setStatus('error')
      })
    return () => {
      cancelled = true
    }
  }, [kind, view, reloadCount, t])

  const showLoading = useCallback(() => {
    setStatus('loading')
    setMessage(null)
  }, [])

  const changeView = (nextView) => {
    if (nextView === view) return
    showLoading()
    setView(nextView)
  }

  const refresh = () => {
    showLoading()
    setReloadCount((count) => count + 1)
  }

  // The backend answers 404 for "not found / not pending" (and, for reject, "created less than one day ago").
  const run = async (type, request) => {
    setAction({ id: request.id, type })
    setMessage(null)
    try {
      await (type === 'approve' ? approve : reject)(kind, request.id)
      setItems((current) => current.filter((item) => item.id !== request.id))
      setMessage({ variant: 'success', text: t(type === 'approve' ? 'admin.messages.approved' : 'admin.messages.rejected', { name: request.name }) })
    } catch (error) {
      if (error.response?.status === 404) {
        setMessage({ variant: 'error', text: t(type === 'approve' ? 'admin.errors.approveNotFound' : 'admin.errors.rejectNotAllowed') })
      } else {
        setMessage({ variant: 'error', text: getErrorMessage(error, t) })
      }
    } finally {
      setAction(null)
    }
  }

  return (
    <div>
      <div className={styles.toolbar}>
        <div className={styles.views} role="group">
          <Button size="sm" variant={view === VIEWS.PENDING ? 'primary' : 'secondary'} onClick={() => changeView(VIEWS.PENDING)} aria-pressed={view === VIEWS.PENDING}>
            {t('admin.tabs.pending')}
          </Button>
          <Button size="sm" variant={view === VIEWS.ALL ? 'primary' : 'secondary'} onClick={() => changeView(VIEWS.ALL)} aria-pressed={view === VIEWS.ALL}>
            {t('admin.tabs.all')}
          </Button>
        </div>
        <Button size="sm" variant="ghost" onClick={refresh} disabled={status === 'loading'}>
          {t('admin.list.refresh')}
        </Button>
      </div>

      {message ? <Alert variant={message.variant}>{message.text}</Alert> : null}

      {status === 'loading' ? (
        <div className={styles.center}>
          <Spinner label={t('common.loading')} />
        </div>
      ) : null}

      {status === 'error' ? (
        <>
          <Alert variant="error">{`${t('admin.errors.loadFailed')} ${loadError}`.trim()}</Alert>
          <Button variant="secondary" size="sm" onClick={refresh}>
            {t('common.retry')}
          </Button>
        </>
      ) : null}

      {status === 'ready' && items.length === 0 ? (
        <p className={styles.empty}>{t(view === VIEWS.PENDING ? 'admin.list.emptyPending' : 'admin.list.emptyAll')}</p>
      ) : null}

      {status === 'ready' && items.length > 0 && view === VIEWS.PENDING ? (
        <div className={styles.grid}>
          {items.map((request) => (
            <RequestCard
              key={request.id}
              request={request}
              busy={action?.id === request.id ? action.type : null}
              disabled={action !== null}
              onApprove={(item) => run('approve', item)}
              onReject={(item) => run('reject', item)}
            />
          ))}
        </div>
      ) : null}

      {status === 'ready' && items.length > 0 && view === VIEWS.ALL ? (
        <>
          <p className={styles.count}>{t('admin.list.count', { count: items.length })}</p>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">{t('admin.list.id')}</th>
                <th scope="col">{t('admin.list.name')}</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td className={styles.idCell}>{item.id}</td>
                  <td>{item.name}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      ) : null}
    </div>
  )
}
