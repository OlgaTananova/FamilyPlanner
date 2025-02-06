// components/ClientLayout.tsx
"use client";

import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { ReactNode, useEffect, useState } from "react";
import msalConfig from "../config/authConfig";

const msalInstance = new PublicClientApplication(msalConfig);

interface ClientLayoutProps {
  children: ReactNode;
}

const MsalPageProvider = ({ children }: ClientLayoutProps) => {
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    const initializeMsal = async () => {
      try {
        await msalInstance.initialize(); // Ensure MSAL is initialized
        setIsInitialized(true);
      } catch (error) {
        console.error("Error initializing MSAL:", error);
      }
    };

    initializeMsal();
  }, []);

  if (!isInitialized) {
    return (
      <div className="flex justify-center items-center h-screen bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50">
        <div className="text-center">
          {/* Spinner */}
          <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-purple-600 border-opacity-50"></div>
          {/* Loading Text */}
          <p className="mt-4 text-xl font-semibold text-purple-700">
            Loading...
          </p>
        </div>
      </div>
    )
  }
  return <MsalProvider instance={msalInstance}>{children}</MsalProvider>;
};

export default MsalPageProvider;
