import { resolveBackendOrigin } from './backendAddress'

export const BACKEND_ORIGIN = resolveBackendOrigin(import.meta.env.VITE_API_BASE_URL)

// Vite forwards development requests to the backend. Built deployments use its address directly.
export const API_BASE_URL = import.meta.env.DEV ? '' : BACKEND_ORIGIN

export const DEFAULT_TIMEOUT_MS = 15000

// Login and "forgot password" send an email inside the request.
// The backend waits up to 30 seconds for the SMTP connection, so we wait longer.
export const EMAIL_REQUEST_TIMEOUT_MS = 45000
