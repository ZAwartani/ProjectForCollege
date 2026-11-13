import { isAccountType } from '../constants/accountTypes'

// Where the login session lives:
// - accessToken: memory only (never written to disk)
// - refreshToken, email, accountType: localStorage, so a page reload keeps you logged in.
//   The backend refresh endpoint needs the email together with the refresh token.

const STORAGE_KEY = 'smartfood.session'
const EMPTY_SESSION = Object.freeze({
  accessToken: null,
  refreshToken: null,
  email: null,
  accountType: null,
})

let snapshot = EMPTY_SESSION
const listeners = new Set()

function emit(next) {
  snapshot = next
  listeners.forEach((listener) => listener())
}

function persist(session) {
  try {
    if (session.refreshToken) {
      const { refreshToken, email, accountType } = session
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ refreshToken, email, accountType }))
    } else {
      localStorage.removeItem(STORAGE_KEY)
    }
  } catch {
    /* storage can be unavailable; the session then lasts until the page closes */
  }
}

export const getSession = () => snapshot

export function subscribe(listener) {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

export function loadPersistedSession() {
  try {
    const stored = JSON.parse(localStorage.getItem(STORAGE_KEY))
    if (stored?.refreshToken && stored?.email && isAccountType(stored.accountType)) {
      return stored
    }
  } catch {
    /* ignore corrupted or unavailable storage */
  }
  return null
}

// Loads the saved refresh data into memory (no access token yet).
export function restoreSession(stored) {
  emit({ ...EMPTY_SESSION, ...stored })
}

export function startSession(session) {
  const next = { ...EMPTY_SESSION, ...session }
  persist(next)
  emit(next)
}

// The backend rotates the refresh token on every refresh, so both tokens are replaced.
export function updateTokens({ accessToken, refreshToken }) {
  startSession({ ...snapshot, accessToken, refreshToken })
}

export function clearSession() {
  persist(EMPTY_SESSION)
  emit(EMPTY_SESSION)
}

// Keep several browser tabs in agreement: another tab may have refreshed or logged out.
if (typeof window !== 'undefined') {
  window.addEventListener('storage', (event) => {
    if (event.key !== null && event.key !== STORAGE_KEY) return
    const stored = loadPersistedSession()
    if (!stored) {
      if (snapshot.refreshToken) emit(EMPTY_SESSION)
    } else if (stored.refreshToken !== snapshot.refreshToken) {
      emit({ ...snapshot, ...stored })
    }
  })
}
