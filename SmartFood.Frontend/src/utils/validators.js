// Client-side checks that give quick feedback. The backend does not validate these fields.
export const MIN_PASSWORD_LENGTH = 8 // a frontend choice for NEW passwords, not a backend rule
export const OTP_LENGTH = 6 // the backend generates codes from 100000 to 999999

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const OTP_PATTERN = new RegExp(`^\\d{${OTP_LENGTH}}$`)

export const isFilled = (value) => value.trim().length > 0
export const isValidEmail = (value) => EMAIL_PATTERN.test(value.trim())
export const isValidOtp = (value) => OTP_PATTERN.test(value.trim())
export const isValidNewPassword = (value) => value.length >= MIN_PASSWORD_LENGTH
