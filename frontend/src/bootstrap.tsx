import { createBrowserHistory } from 'history';
import React from 'react';
import { createRoot } from 'react-dom/client';
import createAppStore from 'Store/createAppStore';
import App from './App/App';

import 'Diag/ConsoleApi';

export async function bootstrap() {
  const history = createBrowserHistory();
  const store = createAppStore(history);
  const container = document.getElementById('root');

  if (container) {
    const root = createRoot(container);
    root.render(<App store={store} history={history} />);
  }
}
