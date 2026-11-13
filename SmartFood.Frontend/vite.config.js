import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The backend only allows CORS from http://localhost:5173,
// so the dev server must always use exactly this port.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
  },
})
