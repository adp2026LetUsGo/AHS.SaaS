import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { resolve } from 'path'

// ADR-008: Resolve AHS.Web.Common shared CSS from the monorepo.
// __dirname = src/Clients/AHS.Xinfer.UI.Demo
// Target    = src/Foundation/AHS.Web.Common/wwwroot
// Path:       ../../Foundation/AHS.Web.Common/wwwroot  (2 levels up)
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      // Usage: import '@ahs-web-common/css/sovereign-elite.css'
      '@ahs-web-common': resolve(__dirname, '..', '..', 'Foundation', 'AHS.Web.Common', 'wwwroot'),
    },
  },
})
