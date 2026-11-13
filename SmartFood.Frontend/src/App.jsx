import { BrowserRouter } from 'react-router-dom'
import AuthProvider from './context/AuthProvider'
import LanguageProvider from './context/LanguageProvider'
import AppRouter from './routes/AppRouter'

export default function App() {
  return (
    <LanguageProvider>
      <AuthProvider>
        <BrowserRouter>
          <AppRouter />
        </BrowserRouter>
      </AuthProvider>
    </LanguageProvider>
  )
}
