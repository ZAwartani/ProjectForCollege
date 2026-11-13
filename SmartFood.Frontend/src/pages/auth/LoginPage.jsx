import { useState } from 'react'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { login as requestOtp, verifyOtp } from '../../api/authService'
import { getErrorMessage } from '../../api/errors'
import AccountTypeTabs from '../../components/auth/AccountTypeTabs'
import AuthCard from '../../components/auth/AuthCard'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { ACCOUNT_TYPES, HOME_PATH_BY_ACCOUNT_TYPE } from '../../constants/accountTypes'
import useAuth from '../../hooks/useAuth'
import useLanguage from '../../hooks/useLanguage'
import { OTP_LENGTH, isFilled, isValidEmail, isValidOtp } from '../../utils/validators'

const STEP = { CREDENTIALS: 'credentials', OTP: 'otp' }

// Login has two steps because the backend requires an email OTP:
// 1) email + password -> backend emails a code, 2) code -> backend returns the tokens.
export default function LoginPage() {
  const { t } = useLanguage()
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams, setSearchParams] = useSearchParams()

  const accountType =
    searchParams.get('type') === ACCOUNT_TYPES.ORGANIZATION
      ? ACCOUNT_TYPES.ORGANIZATION
      : ACCOUNT_TYPES.RESTAURANT

  const [step, setStep] = useState(STEP.CREDENTIALS)
  const [form, setForm] = useState({ email: '', password: '', otp: '' })
  const [fieldErrors, setFieldErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [info, setInfo] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const notice = location.state?.notice

  const updateField = (name) => (event) => {
    setForm((current) => ({ ...current, [name]: event.target.value }))
    setFieldErrors((current) => ({ ...current, [name]: undefined }))
  }

  const changeAccountType = (type) => {
    setSearchParams({ type }, { replace: true, state: null })
    setFormError('')
    setFieldErrors({})
  }

  // Sends the OTP email. Used by step 1 and by "resend code".
  const sendCode = async () => {
    await requestOtp(accountType, { email: form.email.trim(), password: form.password })
  }

  const handleCredentialsSubmit = async (event) => {
    event.preventDefault()
    const errors = {}
    if (!isValidEmail(form.email)) errors.email = 'email'
    if (!isFilled(form.password)) errors.password = 'required'
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) return

    setSubmitting(true)
    setFormError('')
    try {
      await sendCode()
      setStep(STEP.OTP)
      setInfo(t('auth.login.codeSent', { email: form.email.trim() }))
    } catch (error) {
      setFormError(getErrorMessage(error, t))
    } finally {
      setSubmitting(false)
    }
  }

  const handleOtpSubmit = async (event) => {
    event.preventDefault()
    if (!isValidOtp(form.otp)) {
      setFieldErrors({ otp: 'otp' })
      return
    }

    setSubmitting(true)
    setFormError('')
    try {
      const tokens = await verifyOtp(accountType, { email: form.email.trim(), otp: form.otp.trim() })
      const loggedInType = login(tokens, form.email.trim())
      navigate(HOME_PATH_BY_ACCOUNT_TYPE[loggedInType], { replace: true })
    } catch (error) {
      setFormError(error.response ? getErrorMessage(error, t) : error.message || t('errors.unexpected'))
      setSubmitting(false)
    }
  }

  const handleResend = async () => {
    setSubmitting(true)
    setFormError('')
    try {
      await sendCode()
      setInfo(t('auth.login.codeResent'))
    } catch (error) {
      setFormError(getErrorMessage(error, t))
    } finally {
      setSubmitting(false)
    }
  }

  const goBackToCredentials = () => {
    setStep(STEP.CREDENTIALS)
    setForm((current) => ({ ...current, otp: '' }))
    setFormError('')
    setInfo('')
    setFieldErrors({})
  }

  const showError = (name) => (fieldErrors[name] ? t(`auth.validation.${fieldErrors[name]}`) : undefined)

  if (step === STEP.OTP) {
    return (
      <AuthCard title={t('auth.login.otpTitle')} subtitle={t('auth.login.otpSubtitle', { length: OTP_LENGTH })}>
        {info ? <Alert variant="info">{info}</Alert> : null}
        {formError ? <Alert variant="error">{formError}</Alert> : null}

        <form onSubmit={handleOtpSubmit} noValidate>
          <Input
            label={t('auth.fields.otp')}
            name="otp"
            value={form.otp}
            onChange={updateField('otp')}
            error={showError('otp')}
            inputMode="numeric"
            autoComplete="one-time-code"
            maxLength={OTP_LENGTH}
            dir="ltr"
            autoFocus
          />
          <Button type="submit" fullWidth loading={submitting}>
            {t('auth.login.otpSubmit')}
          </Button>
        </form>

        <AuthFooterButtons
          onResend={handleResend}
          onBack={goBackToCredentials}
          disabled={submitting}
          resendLabel={t('auth.login.resend')}
          backLabel={t('auth.login.changeEmail')}
        />
      </AuthCard>
    )
  }

  return (
    <AuthCard
      title={t('auth.login.title')}
      subtitle={t('auth.login.subtitle')}
      footer={
        <>
          <Link to={`/forgot-password?type=${accountType}`}>{t('auth.login.forgot')}</Link>
          <span>
            {t('auth.login.noAccount')} <Link to={`/register?type=${accountType}`}>{t('auth.login.registerLink')}</Link>
          </span>
        </>
      }
    >
      <AccountTypeTabs value={accountType} onChange={changeAccountType} disabled={submitting} />

      {notice ? <Alert variant="success">{t(`auth.notices.${notice}`)}</Alert> : null}
      {formError ? <Alert variant="error">{formError}</Alert> : null}

      <form onSubmit={handleCredentialsSubmit} noValidate>
        <Input
          label={t('auth.fields.email')}
          name="email"
          type="email"
          value={form.email}
          onChange={updateField('email')}
          error={showError('email')}
          autoComplete="email"
          dir="ltr"
        />
        <Input
          label={t('auth.fields.password')}
          name="password"
          type="password"
          value={form.password}
          onChange={updateField('password')}
          error={showError('password')}
          autoComplete="current-password"
          dir="ltr"
        />
        <Button type="submit" fullWidth loading={submitting}>
          {t('auth.login.submit')}
        </Button>
      </form>
    </AuthCard>
  )
}

function AuthFooterButtons({ onResend, onBack, disabled, resendLabel, backLabel }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 'var(--space-4)' }}>
      <Button variant="ghost" size="sm" onClick={onBack} disabled={disabled}>
        {backLabel}
      </Button>
      <Button variant="ghost" size="sm" onClick={onResend} disabled={disabled}>
        {resendLabel}
      </Button>
    </div>
  )
}
