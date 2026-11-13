// Reads the payload of a JWT. The signature is NOT verified here (only the backend can do that).
// This is only used to know who is logged in and which role they have.

const ROLE_CLAIMS = [
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  'role',
]
const EMAIL_CLAIMS = [
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
  'email',
]
const ID_CLAIMS = [
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
  'nameid',
  'sub',
]

function decodeBase64Url(value) {
  const base64 = value.replace(/-/g, '+').replace(/_/g, '/')
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=')
  const binary = atob(padded)
  const bytes = Uint8Array.from(binary, (char) => char.charCodeAt(0))
  return new TextDecoder().decode(bytes)
}

export function decodeJwt(token) {
  try {
    const payload = token.split('.')[1]
    return payload ? JSON.parse(decodeBase64Url(payload)) : null
  } catch {
    return null
  }
}

function firstClaim(payload, names) {
  for (const name of names) {
    const value = payload?.[name]
    if (value !== undefined && value !== null) {
      return Array.isArray(value) ? value[0] : value
    }
  }
  return null
}

export function readTokenInfo(token) {
  const payload = decodeJwt(token)
  if (!payload) return null
  return {
    id: firstClaim(payload, ID_CLAIMS),
    email: firstClaim(payload, EMAIL_CLAIMS),
    role: firstClaim(payload, ROLE_CLAIMS),
    expiresAt: typeof payload.exp === 'number' ? payload.exp * 1000 : null,
  }
}
