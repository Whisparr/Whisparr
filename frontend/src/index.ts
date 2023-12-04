import './polyfills';
import 'Styles/globals.css';
import './index.css';

const initializeUrl = `${
  window.Whisparr.urlBase
}/initialize.json?t=${Date.now()}`;
const response = await fetch(initializeUrl);

window.Whisparr = await response.json();

/* eslint-disable no-undef, @typescript-eslint/ban-ts-comment */
// @ts-ignore 2304
__webpack_public_path__ = `${window.Whisparr.urlBase}/`;
/* eslint-enable no-undef, @typescript-eslint/ban-ts-comment */

const { bootstrap } = await import('./bootstrap');

await bootstrap();
