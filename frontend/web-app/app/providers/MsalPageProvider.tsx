"use client";

import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { ReactNode } from "react";
import msalConfig from "../config/authConfig";

const msalInstance = new PublicClientApplication(msalConfig);

interface MsalPageProviderProps {
  children: ReactNode;
}

const MsalPageProvider = ({ children }: MsalPageProviderProps) => {
  return <MsalProvider instance={msalInstance}>{children}</MsalProvider>;
};

export default MsalPageProvider;
