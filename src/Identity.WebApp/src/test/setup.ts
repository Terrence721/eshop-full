import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'
import '@testing-library/jest-dom/vitest'

// Global, not per-file: every api/*.ts test stubs global fetch via
// vi.stubGlobal, and forgetting to clean it up would leak into the next
// test file since vitest doesn't reset globals between files on its own.
afterEach(() => {
  vi.unstubAllGlobals()
})

// React Testing Library normally auto-registers this cleanup itself, but
// only when it detects a global afterEach -- this project uses explicit
// imports (globals: false), so nothing unmounted components between tests,
// and every render() call accumulated in the same DOM across a test file.
afterEach(() => {
  cleanup()
})
