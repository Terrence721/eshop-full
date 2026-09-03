import { describe, expect, it } from 'vitest'
import { buildExternalChallengeUrl } from './external'

describe('buildExternalChallengeUrl', () => {
  it('includes only the scheme when returnUrl is null', () => {
    expect(buildExternalChallengeUrl('Google', null)).toBe('/External/Challenge?scheme=Google')
  })

  it('includes returnUrl when present', () => {
    expect(buildExternalChallengeUrl('Google', '/connect/authorize/callback')).toBe(
      '/External/Challenge?scheme=Google&returnUrl=%2Fconnect%2Fauthorize%2Fcallback',
    )
  })

  it('URL-encodes special characters in both scheme and returnUrl', () => {
    expect(buildExternalChallengeUrl('My Scheme', '/a b?c=d')).toBe('/External/Challenge?scheme=My+Scheme&returnUrl=%2Fa+b%3Fc%3Dd')
  })
})
