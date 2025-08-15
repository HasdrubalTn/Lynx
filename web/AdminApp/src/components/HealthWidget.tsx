import React, { useState, useEffect } from 'react'

interface HealthStatus {
  status: 'Healthy' | 'Degraded' | 'Unhealthy'
  timestamp: string
  dependencies?: Record<string, 'Healthy' | 'Degraded' | 'Unhealthy'>
  error?: string
}

export default function HealthWidget() {
  const [health, setHealth] = useState<HealthStatus | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const checkHealth = async () => {
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'
    try {
      const response = await fetch(`${apiBaseUrl}/health/ready`)
      if (response.ok) {
        const data: HealthStatus = await response.json()
        setHealth(data)
      } else {
        setHealth({
          status: 'Unhealthy',
          timestamp: new Date().toISOString(),
          error: `API returned ${response.status}`
        })
      }
    } catch (error) {
      setHealth({
        status: 'Unhealthy',
        timestamp: new Date().toISOString(),
        error: error instanceof Error ? error.message : 'Unknown error'
      })
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    // Initial check
    checkHealth()
    
    // Poll every 30 seconds
    const interval = setInterval(checkHealth, 30000)
    return () => clearInterval(interval)
  }, [])

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Healthy': return '#4CAF50'  // Green
      case 'Degraded': return '#FF9800' // Amber
      case 'Unhealthy': return '#F44336' // Red
      default: return '#9E9E9E' // Gray
    }
  }

  const getStatusEmoji = (status: string) => {
    switch (status) {
      case 'Healthy': return '🟢'
      case 'Degraded': return '🟡'
      case 'Unhealthy': return '🔴'
      default: return '⚫'
    }
  }

  if (isLoading) {
    return (
      <div style={{ padding: '16px', border: '1px solid #ddd', borderRadius: '8px' }}>
        <h3>System Health</h3>
        <div>Loading...</div>
      </div>
    )
  }

  return (
    <div style={{ padding: '16px', border: '1px solid #ddd', borderRadius: '8px' }}>
      <h3>System Health</h3>
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '12px' }}>
        <span style={{ fontSize: '24px' }}>
          {getStatusEmoji(health?.status || 'Unknown')}
        </span>
        <span style={{ 
          fontWeight: 'bold', 
          color: getStatusColor(health?.status || 'Unknown') 
        }}>
          {health?.status || 'Unknown'}
        </span>
      </div>
      
      {health?.dependencies && (
        <div>
          <h4 style={{ marginBottom: '8px' }}>Dependencies:</h4>
          {Object.entries(health.dependencies).map(([name, status]) => (
            <div key={name} style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '4px' }}>
              <span>{getStatusEmoji(status)}</span>
              <span style={{ textTransform: 'capitalize' }}>{name}</span>
              <span style={{ color: getStatusColor(status), fontSize: '12px' }}>
                {status}
              </span>
            </div>
          ))}
        </div>
      )}
      
      {health?.error && (
        <div style={{ 
          marginTop: '12px', 
          padding: '8px', 
          backgroundColor: '#ffebee', 
          borderRadius: '4px',
          fontSize: '12px',
          color: '#c62828'
        }}>
          Error: {health.error}
        </div>
      )}
      
      <div style={{ fontSize: '11px', color: '#666', marginTop: '8px' }}>
        Last checked: {health?.timestamp ? new Date(health.timestamp).toLocaleString() : 'Never'}
      </div>
    </div>
  )
}
