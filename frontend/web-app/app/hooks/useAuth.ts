import { useMsal, useIsAuthenticated, useAccount } from "@azure/msal-react";

const loginRequest = {
  scopes: ["openid", "profile", "email"],
};

export const useAuth = () => {
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = useAccount(instance.getAllAccounts()[0] || undefined);

  const signIn = async () => {
    await instance.loginPopup(loginRequest).catch((error) => console.error("Login error:", error));
  };

  const signOut = () => {
    instance.logoutPopup().catch((error) => console.error("Logout error:", error));
  };

  return { signIn, signOut, isAuthenticated, account };
};
