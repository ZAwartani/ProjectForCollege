import { useCallback, useEffect, useMemo, useState, useSyncExternalStore } from 'react'
import { logout as adminLogoutRequest } from '../api/adminService'
import { logout as logoutRequest } from '../api/authService'
import { loadPersistedSession, restoreSession, startSession, clearSession, getSession, subscribe } from '../auth/sessionStore'
import { refreshSession } from '../auth/sessionManager'
import { ACCOUNT_TYPES, ROLE_TO_ACCOUNT_TYPE } from '../constants/accountTypes'
import { readTokenInfo } from '../utils/jwt'
import { AuthContext } from './authContext'

export default function AuthProvider({ children }) {
  const session = useSyncExternalStore(subscribe, getSession)
  // While a saved session is being exchanged for a fresh access token, pages must wait.
  const [restoring, setRestoring] = useState(() => loadPersistedSession() !== null)

  // On page load: if a refresh token was saved, exchange it for a fresh access token.
  useEffect(() => {
    const stored = loadPersistedSession()
    if (!stored) return
    restoreSession(stored)
    refreshSession()
      .catch(() => {})
      .finally(() => setRestoring(false))
  }, [])

  // tokens: { accessToken, refreshToken } returned by verify-otp.
  // Returns the account type so the caller can go to the right home page.
  const login = useCallback((tokens, typedEmail) => {
    const info = readTokenInfo(tokens.accessToken)
    const accountType = ROLE_TO_ACCOUNT_TYPE[info?.role]
    if (!accountType) {
      throw new Error('The access token does not contain a supported role.')
    }
    startSession({
      accessToken: tokens.accessToken,
      refreshToken: tokens.refreshToken,
      email: info.email ?? typedEmail,
      accountType,
    })
    return accountType
  }, [])

  const logout = useCallback(async () => {
    const { accountType, email, refreshToken } = getSession()
    try {
      if (refreshToken) {
        // The admin logout endpoint needs the Authorization header, the other two do not.
        const sendLogout = accountType === ACCOUNT_TYPES.ADMIN ? adminLogoutRequest : (body) => logoutRequest(accountType, body)
        await sendLogout({ email, refreshToken })
      }
    } catch {
      // Even if the server cannot be reached, the user must be logged out locally.
    }
    clearSession()
  }, [])

  const status = restoring ? 'loading' : session.accessToken ? 'authenticated' : 'unauthenticated'

  const value = useMemo(
    () => ({ status, email: session.email, accountType: session.accountType, login, logout }),
    [status, session.email, session.accountType, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
