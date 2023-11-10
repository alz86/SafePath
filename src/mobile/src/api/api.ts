import { BASE_SERVER_URL } from '@env';

// const devUrl = 'https://127.0.0.1:44385/api/app/';
const devUrl = 'https://192.168.40.131:443/api/app/';
const baseServerUrl = devUrl || BASE_SERVER_URL;

export const get = (url: string, data?: any) => {
  const requestUrl = baseServerUrl + url + (data ? `?${buildQueryString(data)}` : '');
  console.log('sending request', { requestUrl });
  return fetch(requestUrl);
};

export const post = (url: string, data?: any) => {
  const requestUrl = baseServerUrl + url + (data ? `?${buildQueryString(data)}` : '');
  console.log('sending request', { requestUrl });
  return fetch(requestUrl, { method: 'POST' });
};

export function buildQueryString(params: Record<string, any>, prefix: string = ''): string {
  const pairs = Object.entries(params).flatMap(([key, value]) => {
    if (typeof value === 'object' && value !== null) {
      if (Array.isArray(value)) {
        return value.map((v, i) => buildQueryString({ [`${key}[${i}]`]: v }, prefix));
      }
      if (value instanceof Date) {
        return `${encodeURIComponent(prefix + key)}=${encodeURIComponent(value.toISOString())}`;
      }
      return buildQueryString(value, `${prefix}${key}.`);
    }
    if (value === undefined || value === null) {
      return `${encodeURIComponent(prefix + key)}=`;
    }
    return `${encodeURIComponent(prefix + key)}=${encodeURIComponent(value.toString())}`;
  });

  return pairs.join('&');
}
