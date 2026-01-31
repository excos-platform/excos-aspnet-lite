/**
 * Get the current path prefix from the URL.
 * Assumes the prefix is the first segment (e.g., /excos)
 */
export function getPathPrefix(): string {
  const path = window.location.pathname;
  const match = path.match(/^\/[^\/]+/);
  return match ? match[0] : '';
}
