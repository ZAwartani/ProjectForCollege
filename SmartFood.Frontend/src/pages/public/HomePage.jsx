import useLanguage from '../../hooks/useLanguage'
import styles from './HomePage.module.css'

export default function HomePage() {
  const { t } = useLanguage()

  return (
    <>
      <section className={styles.hero}>
        <h1>{t('home.title')}</h1>
        <p>{t('home.subtitle')}</p>
      </section>

      <section className={styles.cards}>
        <article className={styles.card}>
          <h2>{t('home.restaurantsTitle')}</h2>
          <p>{t('home.restaurantsText')}</p>
        </article>
        <article className={styles.card}>
          <h2>{t('home.charitiesTitle')}</h2>
          <p>{t('home.charitiesText')}</p>
        </article>
      </section>
    </>
  )
}
