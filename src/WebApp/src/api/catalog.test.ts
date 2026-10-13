import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { catalogItemPictureUrl, getBrands, getCatalogItem, getCatalogItems, getTypes } from './catalog'

describe('getCatalogItems', () => {
  it('requests the given page with no filters', async () => {
    const fetchMock = mockFetchOnce(200, { pageIndex: 0, pageSize: 9, count: 0, data: [] })

    await getCatalogItems(0, 9)

    expect(fetchMock).toHaveBeenCalledWith('/remoteapi/catalog/items?pageIndex=0&pageSize=9', {
      headers: { 'X-CSRF': '1' },
    })
  })

  it('includes brand and type filters when given', async () => {
    const fetchMock = mockFetchOnce(200, { pageIndex: 0, pageSize: 9, count: 0, data: [] })

    await getCatalogItems(1, 9, 2, 3)

    expect(fetchMock).toHaveBeenCalledWith('/remoteapi/catalog/items?pageIndex=1&pageSize=9&brand=2&type=3', {
      headers: { 'X-CSRF': '1' },
    })
  })

  it('resolves with the paginated result', async () => {
    const page = { pageIndex: 0, pageSize: 9, count: 1, data: [{ id: 1, name: 'Mug', description: null, price: 5, catalogTypeId: 1, catalogBrandId: 1 }] }
    mockFetchOnce(200, page)

    await expect(getCatalogItems(0, 9)).resolves.toEqual(page)
  })
})

describe('getCatalogItem', () => {
  it('resolves with the item on success', async () => {
    mockFetchOnce(200, { id: 1, name: 'Mug', description: null, price: 5, catalogTypeId: 1, catalogBrandId: 1 })

    await expect(getCatalogItem(1)).resolves.toMatchObject({ id: 1, name: 'Mug' })
  })

  it('resolves with null when not found', async () => {
    mockFetchOnce(404)

    await expect(getCatalogItem(999)).resolves.toBeNull()
  })

  it('throws for an unexpected failure status', async () => {
    mockFetchOnce(500)

    await expect(getCatalogItem(1)).rejects.toThrow('GET /remoteapi/catalog/items/1 failed: 500')
  })
})

describe('getBrands', () => {
  it('resolves with the brand list', async () => {
    mockFetchOnce(200, [{ id: 1, brand: 'Adventure Works' }])

    await expect(getBrands()).resolves.toEqual([{ id: 1, brand: 'Adventure Works' }])
  })
})

describe('getTypes', () => {
  it('resolves with the type list', async () => {
    mockFetchOnce(200, [{ id: 1, type: 'Mugs' }])

    await expect(getTypes()).resolves.toEqual([{ id: 1, type: 'Mugs' }])
  })
})

describe('catalogItemPictureUrl', () => {
  it('builds the picture URL for an item id', () => {
    expect(catalogItemPictureUrl(42)).toBe('/remoteapi/catalog/items/42/pic')
  })
})
