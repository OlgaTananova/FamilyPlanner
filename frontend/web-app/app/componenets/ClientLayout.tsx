// components/ClientLayout.tsx
"use client";

import { ReactNode } from "react";
import { MsalProvider } from "@azure/msal-react";
import { PublicClientApplication } from "@azure/msal-browser";
import msalConfig from "../config/authConfig";

const msalInstance = new PublicClientApplication(msalConfig);

interface ClientLayoutProps {
  children: ReactNode;
}

const ClientLayout = ({ children }: ClientLayoutProps) => {
  return <MsalProvider instance={msalInstance}>{children}</MsalProvider>;
};

export default ClientLayout;
