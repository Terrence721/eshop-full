import type { ScopeViewModel } from '../api/consent'

export function scopeCheckedMap(vm: { identityScopes: ScopeViewModel[]; apiScopes: ScopeViewModel[] }): Record<string, boolean> {
  const map: Record<string, boolean> = {}
  for (const scope of [...vm.identityScopes, ...vm.apiScopes]) {
    map[scope.value] = scope.checked
  }
  return map
}
