import { Configuration, LogLevel } from "@azure/msal-browser";

const msalConfig: Configuration = {
  auth: {
    clientId: process.env.NEXT_PUBLIC_AZURE_AD_B2C_CLIENT_ID!,
    authority: `https://${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.b2clogin.com/${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.onmicrosoft.com/${process.env.NEXT_PUBLIC_AZURE_AD_B2C_USER_FLOW}`,
    knownAuthorities: [`${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.b2clogin.com`],
    redirectUri: process.env.NEXT_PUBLIC_AZURE_AD_B2C_NGROK_REDIRECT_URI || process.env.NEXT_PUBLIC_AZURE_AD_B2C_REDIRECT_URI,
  },
  cache: {
    cacheLocation: "sessionStorage", // or "localStorage" for persistent sessions
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Info,
    },
  },
};

export default msalConfig;
