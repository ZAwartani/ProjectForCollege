import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { forgotPassword, resetPassword } from '../../api/authService'
import { getErrorMessage } from '../../api/errors'
import AccountTypeTabs from '../../components/auth/AccountTypeTabs'
import AuthCard from '../../components/auth/AuthCard'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { ACCOUNT_TYPES } from '../../constants/accountTypes'
import useLanguage from '../../hooks/useLanguage'
import {
  MIN_PASSWORD_LENGTH,
  OTP_LENGTH,
  isValidEmail,
  isValidNewPassword,
  isValidOtp,
} from '../../utils/validators'

const STEP = { EMAIL: 'email', RESET: 'reset' }

// Two steps: 1) email -> backend emails a code, 2) code + new password -> password changed.
export default function ForgotPasswordPage() {
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()

  const accountType =
    searchParams.get('type') === ACCOUNT_TYPES.ORGANIZATION
      ? ACCOUNT_TYPES.ORGANIZATION
      : ACCOUNT_TYPES.RESTAURANT

  const [step, setStep] = useState(STEP.EMAIL)
  const [form, setForm] = useState({ email: '', otp: '', password: '', confirmPassword: '' })
  const [fieldErrors, setFieldErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [info, setInfo] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const updateField = (name) => (event) => {
    setForm((current) => ({ ...current, [name]: event.target.value }))
    setFieldErrors((current) => ({ ...current, [name]: undefined }))
  }

  const sendCode = async () => {
    await forgotPassword(accountType, { email: form.email.trim() })
  }

  const handleEmailSubmit = async (event) => {
    event.preventDefault()
    if (!isValidEmail(form.email)) {
      setFieldErrors({ email: 'email' })
      return
    }

    setSubmitting(true)
    setFormError('')
    try {
      await sendCode()
      setStep(STEP.RESET)
      setInfo(t('auth.forgot.codeSent', { email: form.email.trim() }))
    } catch (error) {
      setFormError(getErrorMessage(error, t))
    } finally {
      setSubmitting(false)
    }
  }

  const handleResetSubmit = async (event) => {
    event.preventDefault()
    const errors = {}
    if (!isValidOtp(form.otp)) errors.otp = 'otp'
    if (!isValidNewPassword(form.password)) errors.password = 'passwordMin'
    if (form.confirmPassword !== form.password) errors.confirmPassword = 'passwordMismatch'
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) return

    setSubmitting(true)
    setFormError('')
    try {
      await resetPassword(accountType, {
        email: form.email.trim(),
        otp: form.otp.trim(),
        password: form.password,
      })
      navigate(`/login?type=${accountType}`, { state: { notice: 'passwordReset' } })
    } catch (error) {
      setFormError(getErrorMessage(error, t))
      setSubmitting(false)
    }
  }

  const handleResend = async () => {
    setSubmitting(true)
    setFormError('')
    try {
      await sendCode()
      setInfo(t('auth.forgot.codeResent'))
    } catch (error) {
      setFormError(getErrorMessage(error, t))
    } finally {
      setSubmitting(false)
    }
  }

  const showError = (name) =>
    fieldErrors[name] ? t(`auth.validation.${fieldErrors[name]}`, { min: MIN_PASSWORD_LENGTH }) : undefined

  const footer = <Link to={`/login?type=${accountType}`}>{t('auth.forgot.backToLogin')}</Link>

  if (step === STEP.RESET) {
    return (
      <AuthCard title={t('auth.forgot.resetTitle')} subtitle={t('auth.forgot.resetSubtitle', { length: OTP_LENGTH })} footer={footer}>
        {info ? <Alert variant="info">{info}</Alert> : null}
        {formError ? <Alert variant="error">{formError}</Alert> : null}

        <form onSubmit={handleResetSubmit} noValidate>
          <Input
            label={t('auth.fields.otp')}
            value={form.otp}
            onChange={updateField('otp')}
            error={showError('otp')}
            inputMode="numeric"
            autoComplete="one-time-code"
            maxLength={OTP_LENGTH}
            dir="ltr"
            autoFocus
          />
          <Input
            label={t('auth.fields.newPassword')}
            type="password"
            value={form.password}
            onChange={updateField('password')}
            error={showError('password')}
            autoComplete="new-password"
            dir="ltr"
          />
          <Input
            label={t('auth.fields.confirmNewPassword')}
            type="password"
            value={form.confirmPassword}
            onChange={updateField('confirmPassword')}
            error={showError('confirmPassword')}
            autoComplete="new-password"
            dir="ltr"
          />
          <Button type="submit" fullWidth loading={submitting}>
            {t('auth.forgot.resetSubmit')}
          </Button>
        </form>

        <div style={{ marginTop: 'var(--space-4)', textAlign: 'center' }}>
          <Button variant="ghost" size="sm" onClick={handleResend} disabled={submitting}>
            {t('auth.login.resend')}
          </Button>
        </div>
      </AuthCard>
    )
  }

  return (
    <AuthCard title={t('auth.forgot.title')} subtitle={t('auth.forgot.subtitle')} footer={footer}>
      <AccountTypeTabs
        value={accountType}
        onChange={(type) => setSearchParams({ type }, { replace: true })}
        disabled={submitting}
      />

      {formError ? <Alert variant="error">{formError}</Alert> : null}

      <form onSubmit={handleEmailSubmit} noValidate>
        <Input
          label={t('auth.fields.email')}
          type="email"
          value={form.email}
          onChange={updateField('email')}
          error={showError('email')}
          autoComplete="email"
          dir="ltr"
        />
        <Button type="submit" fullWidth loading={submitting}>
          {t('auth.forgot.sendCode')}
        </Button>
      </form>
    </AuthCard>
  )
}
