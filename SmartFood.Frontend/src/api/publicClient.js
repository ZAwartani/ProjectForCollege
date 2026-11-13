import axios from 'axios'
import { API_BASE_URL, DEFAULT_TIMEOUT_MS } from './config'

// Client for endpoints that do not need a token (register, login, refresh...).
// It has no interceptors, so the token-refresh logic can use it without looping.
const publicClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: DEFAULT_TIMEOUT_MS,
})

export default publicClient
