import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AppProviders } from './providers'
import { AdminDashboard } from '../features/admin/AdminDashboard'
import { FakeLoginPage } from '../features/auth/FakeLoginPage'
import { HomePage } from '../features/home/HomePage'
import { PlayerDashboard } from '../features/player/PlayerDashboard'

export function App() {
  return (
    <AppProviders>
      <BrowserRouter>
        <Routes>
          <Route index element={<HomePage />} />
          <Route
            path="player"
            element={
              <FakeLoginPage
                section="player"
                title="Login giocatore"
                description="Accesso finto per iniziare a modellare account, personaggi e stato shard senza backend auth."
                primaryLabel="Entra come player"
                redirectTo="/player/dashboard"
              />
            }
          />
          <Route path="player/dashboard" element={<PlayerDashboard />} />
          <Route
            path="admin"
            element={
              <FakeLoginPage
                section="admin"
                title="Login admin"
                description="Accesso finto per separare subito la console staff dalle superfici pubbliche."
                primaryLabel="Entra come admin"
                redirectTo="/admin/dashboard"
              />
            }
          />
          <Route path="admin/dashboard" element={<AdminDashboard />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AppProviders>
  )
}
