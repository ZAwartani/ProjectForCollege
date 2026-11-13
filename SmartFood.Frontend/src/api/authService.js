import publicClient from './publicClient'
import { ACCOUNT_TYPES } from '../constants/accountTypes'
import { EMAIL_REQUEST_TIMEOUT_MS } from './config'

// Every path and field name below comes from the backend controllers and DTOs
// (see docs/backend-api.md). Restaurant and Organization are NOT identical.
const ENDPOINTS = {
  [ACCOUNT_TYPES.RESTAURANT]: {
    register: '/api/AuthenticationRestaurant/restaurant/register',
    login: '/api/AuthenticationRestaurant/restaurant/login',
    verifyOtp: '/api/AuthenticationRestaurant/restaurant/verify-otp',
    refresh: '/api/AuthenticationRestaurant/restaurant/refresh-token',
    forgotPassword: '/api/AuthenticationRestaurant/restaurant/send-otp',
    resetPassword: '/api/AuthenticationRestaurant/restaurant/reset-password',
    logout: '/api/AuthenticationRestaurant/restaurant/logout',
  },
  [ACCOUNT_TYPES.ORGANIZATION]: {
    register: '/api/AuthenticationOrganization/register',
    login: '/api/AuthenticationOrganization/login',
    verifyOtp: '/api/AuthenticationOrganization/verify-otp',
    refresh: '/api/AuthenticationOrganization/refresh-token',
    forgotPassword: '/api/AuthenticationOrganization/forgot-password',
    resetPassword: '/api/AuthenticationOrganization/reset-password',
    logout: '/api/AuthenticationOrganization/logout',
  },
  // Admin has no OTP. Its login and logout live in adminService.js; only the refresh endpoint is
  // shared here so the common token-refresh code keeps working for every account type.
  [ACCOUNT_TYPES.ADMIN]: {
    refresh: '/api/Admin/refresh-token',
  },
}

// Restaurant answers with plain text, Organization with { message }.
function toMessage(data) {
  if (typeof data === 'string') return data
  return data?.message ?? ''
}

function toTokens(data) {
  if (!data?.accessToken || !data?.refreshToken) {
    throw new Error('The server did not return the expected tokens.')
  }
  return { accessToken: data.accessToken, refreshToken: data.refreshToken }
}

// form: { name, ownerName, registrationNumber, address, email, password }
export async function register(accountType, form) {
  const common = {
    email: form.email,
    password: form.password,
    ownerName: form.ownerName,
    address: form.address,
  }
  const payload =
    accountType === ACCOUNT_TYPES.RESTAURANT
      ? {
          ...common,
          restaurantName: form.name,
          businessRegistrationNumber: form.registrationNumber,
        }
      : {
          ...common,
          organizationName: form.name,
          registrationNumber: form.registrationNumber,
        }

  const { data } = await publicClient.post(ENDPOINTS[accountType].register, payload)
  return toMessage(data)
}

// Step 1 of login: checks the password and makes the backend email an OTP.
export async function login(accountType, { email, password }) {
  const { data } = await publicClient.post(
    ENDPOINTS[accountType].login,
    { email, password },
    { timeout: EMAIL_REQUEST_TIMEOUT_MS },
  )
  return toMessage(data)
}

// Step 2 of login: the OTP property name is different for each account type.
export async function verifyOtp(accountType, { email, otp }) {
  const payload =
    accountType === ACCOUNT_TYPES.RESTAURANT ? { email, otp } : { email, codeOTP: otp }

  const { data } = await publicClient.post(ENDPOINTS[accountType].verifyOtp, payload)
  return toTokens(data)
}

export async function refreshTokens(accountType, { email, refreshToken }) {
  const { data } = await publicClient.post(ENDPOINTS[accountType].refresh, {
    email,
    refreshToken,
  })
  return toTokens(data)
}

export async function forgotPassword(accountType, { email }) {
  const { data } = await publicClient.post(
    ENDPOINTS[accountType].forgotPassword,
    { email },
    { timeout: EMAIL_REQUEST_TIMEOUT_MS },
  )
  return toMessage(data)
}

// "password" is the NEW password.
export async function resetPassword(accountType, { email, otp, password }) {
  const { data } = await publicClient.post(ENDPOINTS[accountType].resetPassword, {
    email,
    password,
    otp,
  })
  return toMessage(data)
}

export async function logout(accountType, { email, refreshToken }) {
  const { data } = await publicClient.post(ENDPOINTS[accountType].logout, {
    email,
    refreshToken,
  })
  return toMessage(data)
}
