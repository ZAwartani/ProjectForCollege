// Turns an axios error into a message for the user.
//
// Why this is more than a status-code switch: the backend throws plain exceptions for business
// errors, so "wrong password" arrives as HTTP 500 with a text body such as
// "System.Exception: Invalid email or password. at ...". We read the exception message from that
// text and translate the ones we know. Anything unknown becomes a generic message.
// This is a workaround until the backend returns proper 400/401 JSON errors.

const KNOWN_MESSAGES = {
  'Invalid email or password.': 'errors.invalidCredentials',
  'Invalid Email Or Password': 'errors.invalidCredentials',
  'Email already exists.': 'errors.emailExists',
  'Restaurant not found.': 'errors.accountNotFound',
  'Organization not found.': 'errors.accountNotFound',
  'Invalid OTP.': 'errors.invalidOtp',
  'OTP expired.': 'errors.otpExpired',
  'Invalid refresh token.': 'errors.sessionExpired',
  'Refresh token is invalid or expired.': 'errors.sessionExpired',
  'Token already revoked.': 'errors.sessionExpired',
  'Refresh token has been revoked.': 'errors.sessionExpired',
  'Refresh token has expired.': 'errors.sessionExpired',
  'Admin not found.': 'errors.sessionExpired',
  'SMTP host is missing.': 'errors.emailNotConfigured',
  'SMTP security is missing.': 'errors.emailNotConfigured',
}

const EXCEPTION_LINE = /(?:^|\n)[\w.]*Exception:\s*([^\r\n]+)/

function firstValidationMessage(errors) {
  const first = Object.values(errors ?? {})[0]
  return Array.isArray(first) ? first[0] : null
}

export function extractServerMessage(data) {
  if (typeof data === 'string') {
    const exception = data.match(EXCEPTION_LINE)
    if (exception) return exception[1].trim()
    const text = data.trim()
    // Short plain strings are real messages; long or HTML bodies are not.
    return text && text.length <= 200 && !text.startsWith('<') ? text : null
  }
  if (data && typeof data === 'object') {
    return data.message ?? firstValidationMessage(data.errors) ?? data.detail ?? data.title ?? null
  }
  return null
}

export function getErrorMessage(error, t) {
  if (error.code === 'ECONNABORTED') return t('errors.timeout')
  if (!error.response) return t('errors.network')

  const { status, data } = error.response
  const serverMessage = extractServerMessage(data)

  if (serverMessage && KNOWN_MESSAGES[serverMessage]) {
    return t(KNOWN_MESSAGES[serverMessage])
  }

  if (status === 401) return t('errors.sessionExpired')
  if (status === 403) return t('errors.forbidden')
  if (status === 404) return serverMessage ?? t('errors.notFound')
  if (status === 409) return serverMessage ?? t('errors.conflict')
  if (status >= 400 && status < 500) return serverMessage ?? t('errors.unexpected')
  return t('errors.server')
}
