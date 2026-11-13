import styles from './Spinner.module.css'

export default function Spinner({ size = 24, label }) {
  return (
    <span className={styles.wrapper} role="status">
      <span className={styles.spinner} style={{ width: size, height: size }} aria-hidden="true" />
      {label ? <span className={styles.label}>{label}</span> : null}
    </span>
  )
}
