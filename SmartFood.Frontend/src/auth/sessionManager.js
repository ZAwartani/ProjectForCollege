import { refreshTokens } from '../api/authService'
import { clearSession, getSession, updateTokens } from './sessionStore'

let inFlightRefresh = null

// Gets a new access token using the refresh token.
// The backend rotates refresh tokens (the old one stops working), so two refreshes at the same
// time would break the session. All callers therefore share one request.
export function refreshSession() {
  if (inFlightRefresh) return inFlightRefresh

  const { refreshToken, email, accountType } = getSession()
  if (!refreshToken) return Promise.reject(new Error('There is no session to refresh.'))

  inFlightRefresh = refreshTokens(accountType, { email, refreshToken })
    .then((tokens) => {
      updateTokens(tokens)
      return tokens.accessToken
    })
    .catch((error) => {
      // The server answered and refused: the session is over.
      // No answer at all (network down): keep the saved session so a later try can work.
      if (error.response) clearSession()
      throw error
    })
    .finally(() => {
      inFlightRefresh = null
    })

  return inFlightRefresh
}
