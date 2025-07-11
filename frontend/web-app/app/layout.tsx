import type { Metadata } from "next";
import "./globals.css";
import Navbar from "./nav/Navbar";
import SideBar from "./nav/SideBar";
import AppInitializer from "./providers/AppInitializer";
import MsalPageProvider from "./providers/MsalPageProvider";
import ReduxProvider from "./providers/ReduxProvider";
import { SignalRProvider } from "./providers/SignalRProvider";
import ToasterProvider from "./providers/ToasterProvider";


const hubUrl = process.env.NEXT_PUBLIC_GATEWAY_URL
  ? `${process.env.NEXT_PUBLIC_GATEWAY_URL}/notifications`
  : "";


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
      <body>
        <ToasterProvider />
        <ReduxProvider>
          <MsalPageProvider>
            <AppInitializer>
              <SignalRProvider hubUrl={hubUrl!}>
                <div className="flex flex-col h-screen ">
                  <Navbar />
                  <div className="flex flex-1">
                    <SideBar />
                    <main className="flex-1 p-4 bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50 relative z-0">
                      {children}
                    </main>
                  </div>
                </div>
              </SignalRProvider>
            </AppInitializer>
          </MsalPageProvider>
        </ReduxProvider>

      </body>

    </html>

  );
}
