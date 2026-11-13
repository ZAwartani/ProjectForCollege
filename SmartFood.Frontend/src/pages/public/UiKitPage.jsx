import useLanguage from '../../hooks/useLanguage'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import Spinner from '../../components/ui/Spinner'
import styles from './UiKitPage.module.css'

// Temporary page used to check the shared components. It will be removed later.
export default function UiKitPage() {
  const { t } = useLanguage()

  return (
    <>
      <h1>{t('uiKit.title')}</h1>
      <p>{t('uiKit.subtitle')}</p>

      <section className={styles.section}>
        <h2>{t('uiKit.buttons')}</h2>
        <div className={styles.row}>
          <Button>{t('uiKit.primary')}</Button>
          <Button variant="secondary">{t('uiKit.secondary')}</Button>
          <Button variant="danger">{t('uiKit.danger')}</Button>
          <Button variant="ghost">{t('uiKit.ghost')}</Button>
          <Button loading>{t('uiKit.loadingButton')}</Button>
          <Button disabled>{t('uiKit.disabledButton')}</Button>
        </div>
      </section>

      <section className={styles.section}>
        <h2>{t('uiKit.inputs')}</h2>
        <div className={styles.form}>
          <Input label={t('uiKit.emailLabel')} type="email" hint={t('uiKit.emailHint')} />
          <Input
            label={t('uiKit.passwordLabel')}
            type="password"
            error={t('uiKit.passwordError')}
          />
        </div>
      </section>

      <section className={styles.section}>
        <h2>{t('uiKit.alerts')}</h2>
        <Alert variant="success">{t('uiKit.success')}</Alert>
        <Alert variant="error">{t('uiKit.error')}</Alert>
        <Alert variant="warning">{t('uiKit.warning')}</Alert>
        <Alert variant="info">{t('uiKit.info')}</Alert>
      </section>

      <section className={styles.section}>
        <h2>{t('uiKit.spinner')}</h2>
        <Spinner label={t('common.loading')} />
      </section>
    </>
  )
}
