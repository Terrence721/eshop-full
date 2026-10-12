import { vi } from 'vitest'

// jsdom doesn't implement EventSource -- shared by every hook/component test
// that renders useReceivedMessages, directly or via a child component.
export class FakeEventSource {
  static instances: FakeEventSource[] = []
  private listeners = new Map<string, Set<(event: MessageEvent) => void>>()
  closed = false
  url: string

  constructor(url: string) {
    this.url = url
    FakeEventSource.instances.push(this)
  }

  addEventListener(type: string, listener: (event: MessageEvent) => void) {
    if (!this.listeners.has(type)) {
      this.listeners.set(type, new Set())
    }
    this.listeners.get(type)?.add(listener)
  }

  close() {
    this.closed = true
  }

  dispatch(type: string) {
    for (const listener of this.listeners.get(type) ?? []) {
      listener(new MessageEvent(type))
    }
  }
}

export function stubEventSource() {
  FakeEventSource.instances = []
  vi.stubGlobal('EventSource', FakeEventSource)
}
