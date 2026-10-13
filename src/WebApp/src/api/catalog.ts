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

// WebBFF's YARP route only strips the "/remoteapi/catalog" prefix off the
// client path (see src/WebBFF/appsettings.json's ReverseProxy config) -- it
// does not also strip "api/catalog", which is Catalog.API's own real route
// group (confirmed in src/Catalog.API/Apis/CatalogApi.cs). So every call
// here needs both segments.
const catalogBase = '/remoteapi/catalog/api/catalog'

export function getCatalogItems(pageIndex: number, pageSize: number, brandId?: number, typeId?: number): Promise<PaginatedItems<CatalogItem>> {
  const params = new URLSearchParams({ pageIndex: String(pageIndex), pageSize: String(pageSize) })
  if (brandId !== undefined) {
    params.set('brand', String(brandId))
  }
  if (typeId !== undefined) {
    params.set('type', String(typeId))
  }
  return apiGet<PaginatedItems<CatalogItem>>(`${catalogBase}/items?${params}`)
}

export function getCatalogItem(id: number): Promise<CatalogItem | null> {
  return apiGet<CatalogItem>(`${catalogBase}/items/${id}`, { treatAsNull: [404] })
}

export function getBrands(): Promise<CatalogBrand[]> {
  return apiGet<CatalogBrand[]>(`${catalogBase}/catalogbrands`)
}

export function getTypes(): Promise<CatalogType[]> {
  return apiGet<CatalogType[]>(`${catalogBase}/catalogtypes`)
}

export function catalogItemPictureUrl(id: number): string {
  return `${catalogBase}/items/${id}/pic`
}
