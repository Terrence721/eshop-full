import { afterEach, vi } from 'vitest'
import '@testing-library/jest-dom/vitest'

// Global, not per-file: every api/*.ts test stubs global fetch via
// vi.stubGlobal, and forgetting to clean it up would leak into the next
// test file since vitest doesn't reset globals between files on its own.
afterEach(() => {
  vi.unstubAllGlobals()
})
