import { apiRoot } from 'Utilities/Fetch/fetchJson';

const getQueryPath = (path: string) => {
  // Whisparr's apiRoot comes from the server as "{urlBase}/api/v3", so it
  // already carries the URL base. Upstream prepends urlBase here because
  // they hardcode a bare '/api/v5'; doing the same would double the base
  // for anyone running behind a reverse proxy.
  return apiRoot + path;
};

export default getQueryPath;
