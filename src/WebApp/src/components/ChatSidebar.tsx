import { useEffect, useRef, useState } from 'react'
import { getCurrentUser, logout } from '../api/bff'
import { useNavigate } from 'react-router'
import { apiPost } from '../lib/apiFetch'

// Mirrors upstream's ChatSidebar.razor but with React + SSE instead of
// SignalR. The container uses `style={{ flex: 1 }}` to fill the full
// sidebar height, which makes the flex-column layout shrink the message
// list and grow the input bar as needed.
export function ChatSidebar() {
  const [claims, setClaims] = useState<UserClaim[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [input, setInput] = useState('')
  const [streaming, setStreaming] = useState(false)
  const eventSourceRef = useRef<EventSource | null>(null)
  const navigate = useNavigate()

  // Type from eShop.Ordering.API.Application.Queries.OrderViewModel
  interface UserClaim {
    type: string
    value: string
  }

  interface ChatMessage {
    role: 'user' | 'assistant'
    content: string
    timestamp: number
  }

  useEffect(() => {
    async function loadUser() {
      const claims = await getCurrentUser()
      setClaims(claims)
      setLoading(false)
    }

    loadUser()
  }, [])

  useEffect(() => {
    if (!claims) return

    // Open SSE stream with the existing Last-Event-ID header
    eventSourceRef.current = new EventSource(`/api/chat/stream?message=${encodeURIComponent(input)}`)

    eventSourceRef.current.onopen = () => {
      setStreaming(true)
    }

    eventSourceRef.current.onerror = (err) => {
      console.error('SSE error:', err)
      eventSourceRef.current?.close()
      setStreaming(false)
    }

    eventSourceRef.current.addEventListener('message', (event) => {
      const delta = event.data
      setMessages(prev => {
        const last = prev[prev.length - 1]
        if (last?.role === 'assistant') {
          const updated = [...prev]
          updated[updated.length - 1] = {
            ...last,
            content: last.content + delta
          }
          return updated
        }
        return [
          ...prev,
          { role: 'assistant', content: delta, timestamp: Date.now() }
        ]
      })
    })

    return () => {
      eventSourceRef.current?.close()
      eventSourceRef.current = null
    }
  }, [claims, input])

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!input.trim() || streaming) return

    const msg = input
    setInput('')
    setMessages(prev => [
      ...prev,
      { role: 'user', content: msg, timestamp: Date.now() }
    ])

    try {
      await apiPost('/api/chat', { body: { Message: msg } })
      // SSE will handle streaming response
    } catch (err) {
      console.error('Failed to send message:', err)
      setMessages(prev => [
        ...prev,
        { role: 'assistant', content: 'Sorry, something went wrong.', timestamp: Date.now() }
      ])
    }
  }

  const handleLogout = async () => {
    await logout()
    navigate('/user/login', { replace: true })
  }

  if (loading) {
    return (
      <div className="sidebar-loading">
        <div className="spinner" />
        <p>Loading...</p>
      </div>
    )
  }

  if (!claims) {
    return (
      <div className="sidebar-unauthenticated">
        <button
          className="action"
          onClick={() => navigate('/user/login', { replace: true })}
        >
          Log in to chat
        </button>
      </div>
    )
  }

  const name = claims.find(c => c.type === 'name')?.value || 'User'

  return (
    <div className="chat-sidebar" style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
      <header className="chat-header" style={{ padding: '1rem', borderBottom: '1px solid #e1e1e1' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <div style={{ width: '40px', height: '40px', borderRadius: '50%', background: '#2556cb', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            {name?.[0].toUpperCase()}
          </div>
          <div>
            <strong>{name}</strong>
            <div style={{ fontSize: '0.875rem', color: '#666' }}>Online</div>
          </div>
        </div>
        <button
          className="action"
          style={{ marginLeft: 'auto', fontSize: '0.875rem', padding: '0.25rem 0.5rem' }}
          onClick={handleLogout}
        >
          Log out
        </button>
      </header>

      <main className="chat-messages" style={{ flex: 1, overflowY: 'auto', padding: '1rem', gap: '1rem', display: 'flex', flexDirection: 'column' }}>
        {messages.map((msg, idx) => (
          <div
            key={idx}
            style={{
              maxWidth: '80%',
              padding: '0.75rem 1rem',
              borderRadius: '1rem',
              background: msg.role === 'user' ? '#2556cb' : '#f1f3f5',
              color: msg.role === 'user' ? '#fff' : '#000',
              alignSelf: msg.role === 'user' ? 'flex-end' : 'flex-start',
              marginBottom: idx === messages.length - 1 && streaming ? '0' : '0.5rem'
            }}
          >
            <p style={{ margin: 0, whiteSpace: 'pre-wrap' }}>{msg.content}</p>
            {msg.role === 'assistant' && idx === messages.length - 1 && streaming && (
              <div style={{ marginTop: '0.5rem', fontSize: '0.875rem', color: '#666' }}>
                <span className="cursor" style={{ animation: 'blink 1s infinite' }}>▋</span>
              </div>
            )}
          </div>
        ))}
      </main>

      <form
        className="chat-input"
        style={{ padding: '1rem', borderTop: '1px solid #e1e1e1', display: 'flex', gap: '0.5rem' }}
        onSubmit={handleSend}
      >
        <input
          type="text"
          value={input}
          onChange={e => setInput(e.target.value)}
          placeholder="Type a message..."
          disabled={streaming}
          style={{
            flex: 1,
            padding: '0.75rem',
            border: '1px solid #e1e1e1',
            borderRadius: '0.5rem',
            fontSize: '1rem'
          }}
        />
        <button
          type="submit"
          className="action"
          disabled={streaming || !input.trim()}
          style={{
            padding: '0.75rem 1.5rem',
            fontSize: '1rem',
            minWidth: '80px'
          }}
        >
          {streaming ? 'Sending...' : 'Send'}
        </button>
      </form>
    </div>
  )
}