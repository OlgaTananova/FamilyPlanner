import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { useAccount, useIsAuthenticated, useMsal } from "@azure/msal-react";
import { useRouter } from "next/navigation";


const catalogReadScope = process.env.NEXT_PUBLIC_AZURE_AD_B2C_CATALOG_READ_SCOPE ?? "";
const catalogWriteScope = process.env.NEXT_PUBLIC_AZURE_AD_B2C_CATALOG_WRITE_SCOPE ?? "";

export const loginRequest = {
  scopes: ["openid",
    "offline_access",
    catalogWriteScope,
    catalogReadScope,
    "profile",
    "email",
  ],

};

export const useAuth = () => {
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = useAccount(instance.getAllAccounts()[0] || undefined);
  const router = useRouter();

  const signIn = async () => {
    //await instance.loginPopup(loginRequest).catch((error) => console.error("Login error:", error));
    await instance.loginRedirect(loginRequest).catch((error) => console.error("Login error:", error));
  };

  const editProfile = async () => {
    await instance.loginRedirect({
      authority: `https://${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.b2clogin.com/${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.onmicrosoft.com/${process.env.NEXT_PUBLIC_AZURE_AD_B2C_PROFILE_EDIT_FLOW}`,
      redirectUri: process.env.NEXT_PUBLIC_AZURE_AD_B2C_EDIT_PROFILE_REDIRECT_URI,
      scopes: ["openid"],
      prompt: "login",
    }).catch(() => console.error("Edit profile redirect error."))
  }

  const signOut = () => {
    instance.logoutPopup().catch((error) => console.error("Logout error:", error));
    router.push("/");
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


  return { signIn, signOut, isAuthenticated, account, acquireToken, editProfile };
};
