import { createContext } from 'react'

// Holds { status, email, accountType, login, logout }. Provided by AuthProvider.
export const AuthContext = createContext(null)
