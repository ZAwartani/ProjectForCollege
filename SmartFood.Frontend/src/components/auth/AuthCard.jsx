import styles from './AuthCard.module.css'

// The white card that holds a login/register/reset form.
export default function AuthCard({ title, subtitle, children, footer }) {
  return (
    <section className={styles.card}>
      <h1 className={styles.title}>{title}</h1>
      {subtitle ? <p className={styles.subtitle}>{subtitle}</p> : null}
      {children}
      {footer ? <div className={styles.footer}>{footer}</div> : null}
    </section>
  )
}
