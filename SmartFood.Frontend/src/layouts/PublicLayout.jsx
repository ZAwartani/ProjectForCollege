import { Outlet } from 'react-router-dom'
import Navbar from '../components/common/Navbar'
import styles from './PublicLayout.module.css'

export default function PublicLayout() {
  return (
    <div className={styles.layout}>
      <Navbar />
      <main className={`container ${styles.main}`}>
        <Outlet />
      </main>
    </div>
  )
}
