import { createContext } from 'react'

// Holds { language, direction, t, toggleLanguage }. Provided by LanguageProvider.
export const LanguageContext = createContext(null)
