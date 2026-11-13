# SmartFood Frontend

React + Vite frontend for the SmartFood platform (restaurants share surplus food with charities).

## Run

```
npm install
npm run dev
```

Open http://localhost:5173 (this port is fixed because the backend CORS only allows it).

## Configuration

Start the ASP.NET backend first. The frontend defaults to its HTTPS profile at
`https://localhost:7270`, including when `.env` is absent after cloning.
To use another backend address, copy `.env.example` to `.env` and set:

```
VITE_API_BASE_URL=https://localhost:7270
```

If running only the HTTP launch profile, use `http://localhost:5288`.
Restart `npm run dev` after changing `.env`. During development Vite forwards
`/api`, `/Restaurants`, and `/Organizations` to that backend. Local HTTPS
development certificates are accepted by this development proxy. Production
builds call the configured backend directly and require valid TLS and CORS settings.

## Structure

- `src/api/` – `config.js` (env + timeouts), `publicClient.js` (no token), `httpClient.js` (adds the
  token, refreshes it on 401), `authService.js` (authentication endpoints), `adminService.js` (admin endpoints), `errors.js`
- `src/auth/` – login session: `sessionStore.js` (where tokens live), `sessionManager.js` (shared refresh)
- `src/components/ui/` – generic components (Button, Input, Alert, Spinner)
- `src/components/common/`, `src/components/auth/`, `src/components/admin/` – Navbar, LanguageToggle, AuthCard, AccountTypeTabs, request cards and lists
- `src/context/` – language and authentication state
- `docs/backend-api.md` – every backend endpoint, DTO field and rule, read from the backend source
- `src/i18n/translations/` – Arabic and English texts
- `src/layouts/`, `src/pages/`, `src/routes/` – page structure and routing
- `src/styles/` – design tokens (`variables.css`) and global styles

## Adding text

Never hardcode visible text in components. Add the key to both `en.js` and `ar.js`
and use `t('section.key')` from `useLanguage()`.

## Status

- [x] Step 1: project setup, design system, Arabic/English with RTL
- [x] Authentication (Restaurant + Organization): register, login with email OTP, forgot/reset
      password, token refresh, logout, role-based routes
- [x] Admin: login (no OTP), pending restaurant/organization requests, approve / reject, all-accounts list
      (hidden page: `/admin/login`, not linked from the public navbar)
- [ ] Restaurant service (profile, location, documents, products)
- [ ] Organization service (profile, location, browse products and restaurants)

## Authentication notes

- The backend answers business errors (wrong password, wrong code...) with HTTP 500 and a text body, so
  `src/api/errors.js` reads the exception message from that text. In a non-Development backend the body is
  empty and the user sees a generic server error until the backend returns proper 400/401 JSON.
- Access token: kept in memory only. Refresh token + email + account type: `localStorage`
  (the backend refresh endpoint needs the email too).
- Set the address to the active backend launch profile. With the HTTPS profile, use its HTTPS
  address directly to avoid HTTP redirects during browser API requests.

## Admin notes

- Reject only works on the backend if the request is Pending AND at least 1 day old; otherwise it answers 404,
  and the page explains that. Rejecting deletes the registration (the backend has no "rejected" state).
- Verification documents are served by the backend as static files; `src/utils/fileUrl.js` turns the stored
  path (Windows backslashes) into a working URL.
- Known backend issue: `POST /api/Admin/register` is open to anyone, so the frontend does not use it.
