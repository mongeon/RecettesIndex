// Blazor WebAssembly service worker (development)
// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the next load). For production, see service-worker.published.js.

self.addEventListener('fetch', () => { });
