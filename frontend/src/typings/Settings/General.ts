export type UpdateMechanism =
  | 'builtIn'
  | 'script'
  | 'external'
  | 'apt'
  | 'docker';

export interface General {
  bindAddress: string;
  port: number;
  urlBase: string;
  applicationUrl: string;
  instanceName: string;
  enableSsl: boolean;
  sslPort: number;
  sslCertPath: string;
  sslKeyPath: string;
  sslCertPassword: string;
  certificateValidation: string;
  launchBrowser: boolean;
  authenticationMethod: string;
  authenticationRequired: string;
  allowedHosts: string;
  trustedNetworks: string;
  analyticsEnabled: boolean;
  username: string;
  password: string;
  passwordConfirmation: string;
  logLevel: string;
  logSizeLimit: number;
  consoleLogLevel: string;
  branch: string;
  apiKey: string;
  updateAutomatically: boolean;
  updateMechanism: UpdateMechanism;
  updateScriptPath: string;
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
