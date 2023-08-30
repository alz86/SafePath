
const yourIP = '192.168.123.36'; // See the docs https://docs.abp.io/en/abp/latest/Getting-Started-React-Native?Tiered=No
const port  = 44385;
const apiUrl = `http://${yourIP}:${port}`;
const ENV = {
  dev: {
    apiUrl: apiUrl,
    oAuthConfig: {
      issuer: apiUrl,
      clientId: 'SafePath_App',
      scope: 'offline_access SafePath',
    },
    localization: {
      defaultResourceName: 'SafePath',
    },
  },
  prod: {
    apiUrl: 'http://localhost:44385',
    oAuthConfig: {
      issuer: 'http://localhost:44385',
      clientId: 'SafePath_App',
      scope: 'offline_access SafePath',
    },
    localization: {
      defaultResourceName: 'SafePath',
    },
  },
};

export const getEnvVars = () => {
  // eslint-disable-next-line no-undef
  return __DEV__ ? ENV.dev : ENV.prod;
};
