import { Route, Routes } from 'react-router-dom'
import PublicLayout from '../layouts/PublicLayout'
import AdminDashboardPage from '../pages/admin/AdminDashboardPage'
import AdminLoginPage from '../pages/admin/AdminLoginPage'
import ForgotPasswordPage from '../pages/auth/ForgotPasswordPage'
import LoginPage from '../pages/auth/LoginPage'
import RegisterPage from '../pages/auth/RegisterPage'
import OrganizationHomePage from '../pages/organization/OrganizationHomePage'
import HomePage from '../pages/public/HomePage'
import NotFoundPage from '../pages/public/NotFoundPage'
import UiKitPage from '../pages/public/UiKitPage'
import RestaurantHomePage from '../pages/restaurant/RestaurantHomePage'
import { ACCOUNT_TYPES } from '../constants/accountTypes'
import GuestRoute from './GuestRoute'
import ProtectedRoute from './ProtectedRoute'

export default function AppRouter() {
  return (
    <Routes>
      <Route element={<PublicLayout />}>
        <Route index element={<HomePage />} />
        <Route path="ui-kit" element={<UiKitPage />} />

        <Route element={<GuestRoute />}>
          <Route path="login" element={<LoginPage />} />
          <Route path="register" element={<RegisterPage />} />
          <Route path="forgot-password" element={<ForgotPasswordPage />} />
          <Route path="admin/login" element={<AdminLoginPage />} />
        </Route>

        <Route element={<ProtectedRoute accountType={ACCOUNT_TYPES.ADMIN} loginPath="/admin/login" />}>
          <Route path="admin" element={<AdminDashboardPage />} />
        </Route>

        <Route element={<ProtectedRoute accountType={ACCOUNT_TYPES.RESTAURANT} />}>
          <Route path="restaurant" element={<RestaurantHomePage />} />
        </Route>

        <Route element={<ProtectedRoute accountType={ACCOUNT_TYPES.ORGANIZATION} />}>
          <Route path="organization" element={<OrganizationHomePage />} />
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
