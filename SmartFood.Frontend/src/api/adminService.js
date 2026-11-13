import httpClient from './httpClient'
import publicClient from './publicClient'
import { getSession } from '../auth/sessionStore'
import { refreshSession } from '../auth/sessionManager'
import { ACCOUNT_TYPES } from '../constants/accountTypes'
import { readTokenInfo } from '../utils/jwt'

// Every path and field name below comes from AdminController and AdminDto.cs (see docs/backend-api.md).
// The restaurant and organization endpoints are alike but NOT identical, so the differences live here.
const KINDS = {
  [ACCOUNT_TYPES.RESTAURANT]: {
    base: '/api/Admin/restaurants',
    nameField: 'restaurantName',
    idField: 'restaurantId', // body field of approve/reject
  },
  [ACCOUNT_TYPES.ORGANIZATION]: {
    base: '/api/Admin/organizations',
    nameField: 'organizationName',
    idField: 'organizationId',
  },
}

// ---------- Authentication (no OTP for the admin) ----------

export async function login({ email, password }) {
  const { data } = await publicClient.post('/api/Admin/login', { email, password })
  if (!data?.accessToken || !data?.refreshToken) {
    throw new Error('The server did not return the expected tokens.')
  }
  return { accessToken: data.accessToken, refreshToken: data.refreshToken }
}

// Unlike the restaurant/organization logout, this one needs the Authorization header
// ([Authorize(Roles = "Admin")]), so it goes through httpClient.
export async function logout({ email, refreshToken }) {
  // If the access token is about to expire, refresh first: the refresh token is rotated, and the
  // request must carry the current one so the right token is revoked.
  const expiresAt = readTokenInfo(getSession().accessToken ?? '')?.expiresAt
  if (expiresAt && expiresAt - Date.now() < 10_000) {
    try {
      await refreshSession()
    } catch {
      /* the request below fails too and the caller logs out locally anyway */
    }
  }
  await httpClient.post('/api/Admin/logout', {
    email,
    refreshToken: getSession().refreshToken ?? refreshToken,
  })
}

// ---------- Registration requests ----------

// The pending lists return ReturnRestaurantDto / ReturnOrganizationDto. One shape for the UI:
function toRequest(kind, dto) {
  return {
    id: dto.id,
    name: dto[KINDS[kind].nameField],
    ownerName: dto.ownerName,
    registrationNumber: dto.registerNumber, // the DTO calls it "registerNumber" for both
    address: dto.address,
    documentPath: dto.verificationDocument?.filePath ?? null,
  }
}

export async function listPending(kind) {
  const { data } = await httpClient.get(`${KINDS[kind].base}/pending`)
  return (Array.isArray(data) ? data : []).map((dto) => toRequest(kind, dto))
}

// ListRestaurantDto / ListOrganizationDto only contain the id and the name.
export async function listAll(kind) {
  const { data } = await httpClient.get(KINDS[kind].base)
  return (Array.isArray(data) ? data : []).map((dto) => ({ id: dto.id, name: dto[KINDS[kind].nameField] }))
}

export async function approve(kind, id) {
  await httpClient.put(`${KINDS[kind].base}/approve`, { [KINDS[kind].idField]: id })
}

// DELETE with a JSON body (the backend reads [FromBody]).
export async function reject(kind, id) {
  await httpClient.delete(`${KINDS[kind].base}/reject`, { data: { [KINDS[kind].idField]: id } })
}
