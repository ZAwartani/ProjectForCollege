import { API_BASE_URL } from '../api/config'

// The backend stores file paths like "Restaurants\Name\VerificationDoc\guid.pdf" (backslashes on
// Windows, relative to wwwroot) and serves wwwroot as static files. Names can contain spaces or
// Arabic letters, so every part of the path is URL-encoded.
export function buildFileUrl(filePath) {
  if (!filePath) return null
  const parts = filePath.replace(/\\/g, '/').split('/').filter(Boolean)
  return `${API_BASE_URL.replace(/\/+$/, '')}/${parts.map(encodeURIComponent).join('/')}`
}
