// The account types that can log in. Restaurant and Organization pick their type on the public login
// page; Admin has its own login page (/admin/login) and is not offered there.
export const ACCOUNT_TYPES = Object.freeze({
  RESTAURANT: 'restaurant',
  ORGANIZATION: 'organization',
  ADMIN: 'admin',
})

// The backend puts one of these strings in the JWT role claim.
export const ROLE_TO_ACCOUNT_TYPE = Object.freeze({
  Restaurant: ACCOUNT_TYPES.RESTAURANT,
  Organization: ACCOUNT_TYPES.ORGANIZATION,
  Admin: ACCOUNT_TYPES.ADMIN,
})

export const HOME_PATH_BY_ACCOUNT_TYPE = Object.freeze({
  [ACCOUNT_TYPES.RESTAURANT]: '/restaurant',
  [ACCOUNT_TYPES.ORGANIZATION]: '/organization',
  [ACCOUNT_TYPES.ADMIN]: '/admin',
})

export function isAccountType(value) {
  return Object.values(ACCOUNT_TYPES).includes(value)
}
