import type { Metadata } from "next";
import "./globals.css";
import Navbar from "./nav/Navbar";
import { MsalProvider } from "@azure/msal-react";
import { PublicClientApplication } from "@azure/msal-browser";
import msalConfig from "./config/authConfig";
import dynamic from "next/dynamic";
import ClientLayout from "./componenets/ClientLayout";
import ToasterProvider from "./providers/ToasterProvider";


export const metadata: Metadata = {
  title: "Family Planner",
  description: "Family Planner",
};


export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (

    <html lang="en">
      <body
        className=""
      >
        <ToasterProvider />
          <ClientLayout>
            <Navbar />
            <main>
              {children}
            </main>
          </ClientLayout>
      </body>

    </html>

  );
}
