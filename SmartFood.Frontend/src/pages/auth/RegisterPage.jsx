import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { register } from '../../api/authService'
import { getErrorMessage } from '../../api/errors'
import AccountTypeTabs from '../../components/auth/AccountTypeTabs'
import AuthCard from '../../components/auth/AuthCard'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { ACCOUNT_TYPES } from '../../constants/accountTypes'
import useLanguage from '../../hooks/useLanguage'
import { MIN_PASSWORD_LENGTH, isFilled, isValidEmail, isValidNewPassword } from '../../utils/validators'

const EMPTY_FORM = {
  name: '',
  ownerName: '',
  registrationNumber: '',
  address: '',
  email: '',
  password: '',
  confirmPassword: '',
}

export default function RegisterPage() {
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()

  const accountType =
    searchParams.get('type') === ACCOUNT_TYPES.ORGANIZATION
      ? ACCOUNT_TYPES.ORGANIZATION
      : ACCOUNT_TYPES.RESTAURANT
  const isRestaurant = accountType === ACCOUNT_TYPES.RESTAURANT

  const [form, setForm] = useState(EMPTY_FORM)
  const [fieldErrors, setFieldErrors] = useState({})
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const updateField = (name) => (event) => {
    setForm((current) => ({ ...current, [name]: event.target.value }))
    setFieldErrors((current) => ({ ...current, [name]: undefined }))
  }

  const validate = () => {
    const errors = {}
    ;['name', 'ownerName', 'registrationNumber', 'address'].forEach((field) => {
      if (!isFilled(form[field])) errors[field] = 'required'
    })
    if (!isValidEmail(form.email)) errors.email = 'email'
    if (!isValidNewPassword(form.password)) errors.password = 'passwordMin'
    if (form.confirmPassword !== form.password) errors.confirmPassword = 'passwordMismatch'
    return errors
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    const errors = validate()
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) return

    setSubmitting(true)
    setFormError('')
    try {
      await register(accountType, {
        name: form.name.trim(),
        ownerName: form.ownerName.trim(),
        registrationNumber: form.registrationNumber.trim(),
        address: form.address.trim(),
        email: form.email.trim(),
        password: form.password,
      })
      navigate(`/login?type=${accountType}`, { state: { notice: 'registered' } })
    } catch (error) {
      setFormError(getErrorMessage(error, t))
      setSubmitting(false)
    }
  }

  const showError = (name) =>
    fieldErrors[name] ? t(`auth.validation.${fieldErrors[name]}`, { min: MIN_PASSWORD_LENGTH }) : undefined

  return (
    <AuthCard
      title={t('auth.register.title')}
      subtitle={t('auth.register.subtitle')}
      footer={
        <span>
          {t('auth.register.haveAccount')} <Link to={`/login?type=${accountType}`}>{t('auth.register.loginLink')}</Link>
        </span>
      }
    >
      <AccountTypeTabs
        value={accountType}
        onChange={(type) => setSearchParams({ type }, { replace: true })}
        disabled={submitting}
      />

      {formError ? <Alert variant="error">{formError}</Alert> : null}

      <form onSubmit={handleSubmit} noValidate>
        <Input
          label={t(isRestaurant ? 'auth.fields.restaurantName' : 'auth.fields.organizationName')}
          value={form.name}
          onChange={updateField('name')}
          error={showError('name')}
        />
        <Input
          label={t('auth.fields.ownerName')}
          value={form.ownerName}
          onChange={updateField('ownerName')}
          error={showError('ownerName')}
        />
        <Input
          label={t(isRestaurant ? 'auth.fields.businessRegistrationNumber' : 'auth.fields.registrationNumber')}
          value={form.registrationNumber}
          onChange={updateField('registrationNumber')}
          error={showError('registrationNumber')}
          dir="ltr"
        />
        <Input
          label={t('auth.fields.address')}
          value={form.address}
          onChange={updateField('address')}
          error={showError('address')}
        />
        <Input
          label={t('auth.fields.email')}
          type="email"
          value={form.email}
          onChange={updateField('email')}
          error={showError('email')}
          autoComplete="email"
          dir="ltr"
        />
        <Input
          label={t('auth.fields.password')}
          type="password"
          value={form.password}
          onChange={updateField('password')}
          error={showError('password')}
          autoComplete="new-password"
          dir="ltr"
        />
        <Input
          label={t('auth.fields.confirmPassword')}
          type="password"
          value={form.confirmPassword}
          onChange={updateField('confirmPassword')}
          error={showError('confirmPassword')}
          autoComplete="new-password"
          dir="ltr"
        />
        <Button type="submit" fullWidth loading={submitting}>
          {t('auth.register.submit')}
        </Button>
      </form>
    </AuthCard>
  )
}
