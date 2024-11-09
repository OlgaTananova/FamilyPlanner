import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { useMsal, useIsAuthenticated, useAccount } from "@azure/msal-react";
import msalConfig from "../config/authConfig";

const loginRequest = {
  scopes: ["openid",
    "offline_access",
    "https://OlgaTananova.onmicrosoft.com/planner-api/catalog.read",
    "https://OlgaTananova.onmicrosoft.com/planner-api/catalog.write",
    "profile",
    "email"
  ],

};

export const useAuth = () => {
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = useAccount(instance.getAllAccounts()[0] || undefined);

  const signIn = async () => {
    //await instance.loginPopup(loginRequest).catch((error) => console.error("Login error:", error));
    await instance.loginRedirect(loginRequest).catch((error) => console.error("Login error:", error));
  };

  const signOut = () => {
    instance.logoutPopup().catch((error) => console.error("Logout error:", error));
  };

  const acquireToken = async (): Promise<string | null> => {
    try {
      const account = instance.getAllAccounts()[0];
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account,
      });
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        const response = await instance.acquireTokenPopup(loginRequest);
        return response.accessToken;
      } else {
        console.error("Token acquisition failed:", error);
        return null;
      }
    }
  }

  return { signIn, signOut, isAuthenticated, account, acquireToken};
};
