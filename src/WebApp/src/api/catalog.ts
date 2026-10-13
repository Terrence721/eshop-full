import { apiGet } from '../lib/apiFetch'

// Matches eShop.Catalog.API.Model.CatalogItem's real server-side shape --
// there is no PictureUrl field; the picture comes from the separate
// /items/{id}/pic endpoint instead (see catalogItemPictureUrl below).
export interface CatalogItem {
  id: number
  name: string
  description: string | null
  price: number
  catalogTypeId: number
  catalogBrandId: number
}

export interface CatalogBrand {
  id: number
  brand: string
}

export interface CatalogType {
  id: number
  type: string
}

export interface PaginatedItems<T> {
  pageIndex: number
  pageSize: number
  count: number
  data: T[]
}

export function getCatalogItems(pageIndex: number, pageSize: number, brandId?: number, typeId?: number): Promise<PaginatedItems<CatalogItem>> {
  const params = new URLSearchParams({ pageIndex: String(pageIndex), pageSize: String(pageSize) })
  if (brandId !== undefined) {
    params.set('brand', String(brandId))
  }
  if (typeId !== undefined) {
    params.set('type', String(typeId))
  }
  return apiGet<PaginatedItems<CatalogItem>>(`/remoteapi/catalog/items?${params}`)
}

export function getCatalogItem(id: number): Promise<CatalogItem | null> {
  return apiGet<CatalogItem>(`/remoteapi/catalog/items/${id}`, { treatAsNull: [404] })
}

export function getBrands(): Promise<CatalogBrand[]> {
  return apiGet<CatalogBrand[]>('/remoteapi/catalog/catalogbrands')
}

export function getTypes(): Promise<CatalogType[]> {
  return apiGet<CatalogType[]>('/remoteapi/catalog/catalogtypes')
}

export function catalogItemPictureUrl(id: number): string {
  return `/remoteapi/catalog/items/${id}/pic`
}
