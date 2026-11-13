// Match the HTTPS profile in SmartFood.Presentation/Properties/launchSettings.json.
export function resolveBackendOrigin(value) {
  const url = new URL(value?.trim() || 'https://localhost:7270')

  if (!['http:', 'https:'].includes(url.protocol) || url.username || url.password) {
    throw new Error('VITE_API_BASE_URL must be an HTTP or HTTPS backend address.')
  }

  if (['localhost', '127.0.0.1', '[::1]'].includes(url.hostname) && url.port === '5173') {
    throw new Error('VITE_API_BASE_URL points to the frontend. Use backend port 5288 or 7270.')
  }

  // API functions already include /api; a pasted Swagger URL must not prefix those paths.
  return url.origin
}
