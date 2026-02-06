export type UpdateMechanism = 'builtIn' | 'script' | 'external' | 'apt' | 'docker';

export interface General {
  bindAddress: string;
  port: number;
  urlBase: string;
  instanceName: string;
  enableSsl: boolean;
  sslPort: number;
  sslCertPath: string;
  sslCertPassword: string;
  launchBrowser: boolean;
  authenticationMethod: string;
  authenticationRequired: string;
  analyticsEnabled: boolean;
  username: string;
  password: string;
  logLevel: string;
  consoleLogLevel: string;
  branch: string;
  apiKey: string;
  updateMechanism: UpdateMechanism;
  updateScriptPath: string;
  updateAutomatically: boolean;
  proxyEnabled: boolean;
  proxyType: string;
  proxyHostname: string;
  proxyPort: number;
  proxyUsername: string;
  proxyPassword: string;
  proxyBypassFilter: string;
  proxyBypassLocalAddresses: boolean;
  backupFolder: string;
  backupInterval: number;
  backupRetention: number;
}

export default General;
