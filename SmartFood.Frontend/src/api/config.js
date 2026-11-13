export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

if (!API_BASE_URL) {
  console.error('VITE_API_BASE_URL is missing. Copy .env.example to .env and set it.')
}

export const DEFAULT_TIMEOUT_MS = 15000

// Login and "forgot password" send an email inside the request.
// The backend waits up to 30 seconds for the SMTP connection, so we wait longer.
export const EMAIL_REQUEST_TIMEOUT_MS = 45000
