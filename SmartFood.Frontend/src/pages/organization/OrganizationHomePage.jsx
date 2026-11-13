import Alert from '../../components/ui/Alert'
import useAuth from '../../hooks/useAuth'
import useLanguage from '../../hooks/useLanguage'

// Temporary landing page after an organization logs in. The real dashboard comes with the organization service.
export default function OrganizationHomePage() {
  const { t } = useLanguage()
  const { email } = useAuth()

  return (
    <section>
      <h1>{t('dashboard.organizationTitle')}</h1>
      <p>{t('dashboard.signedInAs', { email })}</p>
      <Alert variant="info">{t('dashboard.comingSoon')}</Alert>
    </section>
  )
}
