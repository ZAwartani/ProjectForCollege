import { Link } from 'react-router-dom'
import useLanguage from '../../hooks/useLanguage'

export default function NotFoundPage() {
  const { t } = useLanguage()

  return (
    <section>
      <h1>{t('notFound.title')}</h1>
      <p>{t('notFound.text')}</p>
      <Link to="/">{t('notFound.back')}</Link>
    </section>
  )
}
