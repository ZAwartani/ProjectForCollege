import Alert from '../../components/ui/Alert'
import useAuth from '../../hooks/useAuth'
import useLanguage from '../../hooks/useLanguage'

// Temporary landing page after a restaurant logs in. The real dashboard comes with the restaurant service.
export default function RestaurantHomePage() {
  const { t } = useLanguage()
  const { email } = useAuth()

  return (
    <section>
      <h1>{t('dashboard.restaurantTitle')}</h1>
      <p>{t('dashboard.signedInAs', { email })}</p>
      <Alert variant="info">{t('dashboard.comingSoon')}</Alert>
    </section>
  )
}
