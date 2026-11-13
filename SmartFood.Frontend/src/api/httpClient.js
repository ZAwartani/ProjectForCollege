import axios from 'axios'
import { API_BASE_URL, DEFAULT_TIMEOUT_MS } from './config'
import { getSession } from '../auth/sessionStore'
import { refreshSession } from '../auth/sessionManager'

// The single client for endpoints that need a login (everything except authentication).
// - Adds "Authorization: Bearer <token>" to every request.
// - If the backend answers 401 (token expired), refreshes the token once and repeats the request.
// - Multipart uploads: pass a FormData object as the body; axios sets the correct
//   Content-Type (with boundary) by itself, so do not set it manually.
const httpClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: DEFAULT_TIMEOUT_MS,
})

httpClient.interceptors.request.use((config) => {
  const { accessToken } = getSession()
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config
    const canRetry =
      error.response?.status === 401 &&
      original &&
      !original._alreadyRetried &&
      getSession().refreshToken

    if (!canRetry) throw error

    original._alreadyRetried = true
    try {
      const accessToken = await refreshSession()
      original.headers.Authorization = `Bearer ${accessToken}`
      return httpClient(original)
    } catch {
      // Refresh failed: the session store has already been cleared if the server refused.
      throw error
    }
  },
)

export default httpClient
