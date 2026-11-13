import useLanguage from '../../hooks/useLanguage'
import Button from '../ui/Button'

export default function LanguageToggle() {
  const { t, toggleLanguage } = useLanguage()

  return (
    <Button
      variant="secondary"
      size="sm"
      onClick={toggleLanguage}
      aria-label={t('language.switchLabel')}
    >
      {t('language.switchTo')}
    </Button>
  )
}
