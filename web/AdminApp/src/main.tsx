import React from 'react'
import { createRoot } from 'react-dom/client'

function App() {
  return <div>Lynx AdminApp (RBAC required)</div>
}

createRoot(document.getElementById('root')!).render(<App />)
