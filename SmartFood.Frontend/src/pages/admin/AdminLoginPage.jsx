import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { login as adminLogin } from '../../api/adminService'
import { getErrorMessage } from '../../api/errors'
import AuthCard from '../../components/auth/AuthCard'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { HOME_PATH_BY_ACCOUNT_TYPE } from '../../constants/accountTypes'
import useAuth from '../../hooks/useAuth'
import useLanguage from '../../hooks/useLanguage'
import { isFilled, isValidEmail } from '../../utils/validators'

// The admin signs in with email + password only (the backend has no OTP step for admins).
// This page is not linked from the public navigation.
export default function AdminLoginPage() {
  const { t } = useLanguage()
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [loading, setLoading] = useState(false)

  const validate = () => {
    const next = {}
    if (!isFilled(email)) next.email = 'required'
    else if (!isValidEmail(email)) next.email = 'email'
    if (!isFilled(password)) next.password = 'required'
    setErrors(next)
    return Object.keys(next).length === 0
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    setFormError('')
    if (!validate()) return
    setLoading(true)
    try {
      const tokens = await adminLogin({ email: email.trim(), password })
      const accountType = login(tokens, email.trim())
      navigate(HOME_PATH_BY_ACCOUNT_TYPE[accountType], { replace: true })
    } catch (error) {
      setFormError(getErrorMessage(error, t))
      setLoading(false)
    }
  }

  return (
    <AuthCard title={t('admin.login.title')} subtitle={t('admin.login.subtitle')}>
      {formError ? <Alert variant="error">{formError}</Alert> : null}
      <form onSubmit={handleSubmit} noValidate>
        <Input
          label={t('auth.fields.email')}
          type="email"
          autoComplete="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          error={errors.email ? t(`auth.validation.${errors.email}`) : undefined}
        />
        <Input
          label={t('auth.fields.password')}
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          error={errors.password ? t(`auth.validation.${errors.password}`) : undefined}
        />
        <Button type="submit" fullWidth loading={loading}>
          {t('admin.login.submit')}
        </Button>
      </form>
    </AuthCard>
  )
}
