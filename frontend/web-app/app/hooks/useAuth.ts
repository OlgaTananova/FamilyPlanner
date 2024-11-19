import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { useMsal, useIsAuthenticated, useAccount } from "@azure/msal-react";

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

  const acquireToken = async (): Promise<{ accessToken: string | null; idTokenClaims: any | null }> => {
    try {
      const account = instance.getAllAccounts()[0];
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account,
      });
      return {
        accessToken: response.accessToken,
        idTokenClaims: response.idTokenClaims, // Get ID token claims
      };
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        const response = await instance.acquireTokenPopup(loginRequest);
        return {
          accessToken: response.accessToken,
          idTokenClaims: response.idTokenClaims,
        };
      } else {
        console.error("Token acquisition failed:", error);
        return { accessToken: null, idTokenClaims: null };
      }
    }
  };


  return { signIn, signOut, isAuthenticated, account, acquireToken };
};
