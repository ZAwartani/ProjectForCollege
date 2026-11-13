import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'
import { resolveBackendOrigin } from './src/api/backendAddress.js'

// Keep the team's frontend URL stable; forward API calls through the dev server.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const target = resolveBackendOrigin(env.VITE_API_BASE_URL)
  const localBackend = ['localhost', '127.0.0.1', '[::1]'].includes(new URL(target).hostname)
  const proxy = {
    target,
    changeOrigin: true,
    // Trust ASP.NET development certificates only for the local development proxy.
    secure: !localBackend,
    proxyTimeout: 60000,
    configure(server) {
      server.on('error', (_error, _request, response) => {
        if (response && !response.headersSent && typeof response.writeHead === 'function') {
          response.writeHead(503, { 'Content-Type': 'application/json' })
          response.end(JSON.stringify({ code: 'BACKEND_UNAVAILABLE' }))
        }
      })
    },
  }

  return {
    plugins: [react()],
    server: {
      port: 5173,
      strictPort: true,
      proxy: {
        '/api': proxy,
        '/Restaurants': proxy,
        '/Organizations': proxy,
      },
    },
  }
})
