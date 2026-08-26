"use client";

import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { ReactNode, useEffect, useState } from "react";
import msalConfig from "../config/authConfig";

const msalInstance = new PublicClientApplication(msalConfig);

interface MsalPageProviderProps {
  children: ReactNode;
}

const MsalPageProvider = ({ children }: MsalPageProviderProps) => {
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    let isMounted = true;

    msalInstance
      .initialize()
      .then(() => {
        if (isMounted) {
          setIsInitialized(true);
        }
      })
      .catch((error) => {
        console.error("Failed to initialize MSAL:", error);
      });

    return () => {
      isMounted = false;
    };
  }, []);

  if (!isInitialized) {
    return null;
  }

  return (
    <MsalProvider instance={msalInstance}>
      {children}
    </MsalProvider>
  );
};

export default MsalPageProvider;
