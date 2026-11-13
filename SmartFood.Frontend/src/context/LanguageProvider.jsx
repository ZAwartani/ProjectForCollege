import { useCallback, useEffect, useMemo, useState } from 'react'
import en from '../i18n/translations/en'
import ar from '../i18n/translations/ar'
import { LanguageContext } from './languageContext'

const dictionaries = { en, ar }
const STORAGE_KEY = 'smartfood.language'
const DEFAULT_LANGUAGE = 'ar'

function readStoredLanguage() {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    return stored in dictionaries ? stored : DEFAULT_LANGUAGE
  } catch {
    return DEFAULT_LANGUAGE
  }
}

// Finds "a.b.c" inside a nested object. Returns undefined if missing.
function lookup(dictionary, key) {
  return key.split('.').reduce((node, part) => node?.[part], dictionary)
}

export default function LanguageProvider({ children }) {
  const [language, setLanguage] = useState(readStoredLanguage)
  const direction = language === 'ar' ? 'rtl' : 'ltr'

  // Keep the <html> tag in sync so the browser flips the layout for RTL.
  useEffect(() => {
    document.documentElement.lang = language
    document.documentElement.dir = direction
    try {
      localStorage.setItem(STORAGE_KEY, language)
    } catch {
      /* storage can be unavailable (private mode); the app still works */
    }
  }, [language, direction])

  const t = useCallback(
    (key, params = {}) => {
      const text = lookup(dictionaries[language], key) ?? lookup(en, key) ?? key
      return Object.entries(params).reduce(
        (result, [name, value]) => result.replaceAll(`{${name}}`, value),
        text,
      )
    },
    [language],
  )

  const toggleLanguage = useCallback(() => {
    setLanguage((current) => (current === 'ar' ? 'en' : 'ar'))
  }, [])

  const value = useMemo(
    () => ({ language, direction, t, toggleLanguage }),
    [language, direction, t, toggleLanguage],
  )

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>
}
