import styles from './Alert.module.css'

// variant: success | error | warning | info
export default function Alert({ variant = 'info', children }) {
  const role = variant === 'error' ? 'alert' : 'status'
  return (
    <div className={`${styles.alert} ${styles[variant]}`} role={role}>
      {children}
    </div>
  )
}
