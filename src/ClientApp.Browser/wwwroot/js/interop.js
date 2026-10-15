export function getCurrentUrl() {
    return globalThis.window.location.href;
}

export function navigateTo(url) {
    globalThis.window.location.assign(url);
}

export function replaceUrl(url) {
    globalThis.window.history.replaceState(null, '', url);
}

export function sessionStorageGet(key) {
    return globalThis.window.sessionStorage.getItem(key);
}

export function sessionStorageSet(key, value) {
    globalThis.window.sessionStorage.setItem(key, value);
}

export function sessionStorageRemove(key) {
    globalThis.window.sessionStorage.removeItem(key);
}
