import React from 'react'
import { createRoot } from 'react-dom/client'
import HealthWidget from './components/HealthWidget'

function App() {
  return (
    <div>
      <h1>Lynx AdminApp (RBAC required)</h1>
      <HealthWidget />
    </div>
  )
}

createRoot(document.getElementById('root')!).render(<App />)
