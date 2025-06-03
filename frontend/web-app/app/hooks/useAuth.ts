import { AuthenticationResult, InteractionRequiredAuthError } from "@azure/msal-browser";
import { useAccount, useIsAuthenticated, useMsal } from "@azure/msal-react";
import { useRouter } from "next/navigation";


const catalogReadScope = process.env.NEXT_PUBLIC_AZURE_AD_B2C_CATALOG_READ_SCOPE ?? "";
const catalogWriteScope = process.env.NEXT_PUBLIC_AZURE_AD_B2C_CATALOG_WRITE_SCOPE ?? "";
const shopListWriteScope = process.env.NEXT_PUBLIC_AZURE_AD_B2C_SHOPLIST_WRITE_SCOPE ?? "";
const shopListReadScope = process.env.NEXT_PUBLIC_AZURE_AD_B2C_SHOPLIST_READ_SCOPE ?? "";


export const loginRequest = {
  scopes: ["openid",
    "offline_access",
    catalogWriteScope,
    catalogReadScope,
    shopListReadScope,
    shopListWriteScope,
    "profile",
    "email",
  ].filter(Boolean),

};

export const useAuth = () => {
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = useAccount(instance.getAllAccounts()[0] || undefined);
  const router = useRouter();

  const signIn = async () => {
    try {
      await instance.loginRedirect(loginRequest);
    } catch (error) {
      console.error("Login redirect error:", error);
    }
  };

  const editProfile = async () => {
    try {
      await instance.loginRedirect({
        authority: `https://${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.b2clogin.com/${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.onmicrosoft.com/${process.env.NEXT_PUBLIC_AZURE_AD_B2C_PROFILE_EDIT_FLOW}`,
        redirectUri: process.env.NEXT_PUBLIC_AZURE_AD_B2C_EDIT_PROFILE_REDIRECT_URI,
        scopes: ["openid"],
        prompt: "login",
      });
    } catch (error) {
      console.error("Edit profile redirect error:", error);
    }
  }

  const signOut = () => {
    try {
      instance.logoutPopup();
      router.push("/");
    } catch (error) {
      console.error("Logout error:", error);
    }
  };

  const acquireToken = async (): Promise<{ accessToken: string | null; idTokenClaims: any | null }> => {
    const accounts = instance.getAllAccounts();

    // Case 1: No signed-in user
    if (accounts.length === 0) {
      console.warn("No accounts found — prompting login.");
      try {
        const loginResult = await instance.loginPopup(loginRequest);
        return {
          accessToken: loginResult.accessToken,
          idTokenClaims: loginResult.idTokenClaims,
        };
      } catch (loginError) {
        console.error("Login popup failed:", loginError);
        return { accessToken: null, idTokenClaims: null };
      }
    }

    const account = accounts[0];

    // Case 2: Silent attempt
    try {
      const tokenResult: AuthenticationResult = await instance.acquireTokenSilent({
        ...loginRequest,
        account,
      });
      return {
        accessToken: tokenResult.accessToken,
        idTokenClaims: tokenResult.idTokenClaims,
      };
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        try {
          const tokenResult = await instance.acquireTokenPopup({
            ...loginRequest,
            account,
          });
          return {
            accessToken: tokenResult.accessToken,
            idTokenClaims: tokenResult.idTokenClaims,
          };
        } catch (popupError) {
          console.error("Interactive token acquisition failed:", popupError);
          return { accessToken: null, idTokenClaims: null };
        }
      }


      console.error("Token acquisition error:", error);
      return { accessToken: null, idTokenClaims: null };
    }
  };


  return { signIn, signOut, isAuthenticated, account, acquireToken, editProfile, instance };
};
