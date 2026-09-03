import { describe, expect, it } from 'vitest'
import type { ScopeViewModel } from '../api/consent'
import { scopeCheckedMap } from './scopeCheckedMap'

function scope(overrides: Partial<ScopeViewModel> = {}): ScopeViewModel {
  return {
    value: 'openid',
    displayName: 'Your user identifier',
    description: null,
    emphasize: false,
    required: false,
    checked: false,
    ...overrides,
  }
}

describe('scopeCheckedMap', () => {
  it('maps each identity and API scope value to its checked state', () => {
    const result = scopeCheckedMap({
      identityScopes: [scope({ value: 'openid', checked: true }), scope({ value: 'profile', checked: false })],
      apiScopes: [scope({ value: 'orders', checked: true })],
    })

    expect(result).toEqual({ openid: true, profile: false, orders: true })
  })

  it('returns an empty map when there are no scopes', () => {
    expect(scopeCheckedMap({ identityScopes: [], apiScopes: [] })).toEqual({})
  })

  it('keeps the last checked state when identity and API scopes share a value', () => {
    const result = scopeCheckedMap({
      identityScopes: [scope({ value: 'shared', checked: false })],
      apiScopes: [scope({ value: 'shared', checked: true })],
    })

    expect(result).toEqual({ shared: true })
  })
})
