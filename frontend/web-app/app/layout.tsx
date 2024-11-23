import type { Metadata } from "next";
import "./globals.css";
import Navbar from "./nav/Navbar";
import MsalPageProvider from "./providers/MsalPageProvider";
import ToasterProvider from "./providers/ToasterProvider";
import ReduxProvider from "./providers/ReduxProvider";
import { Sidebar } from "flowbite-react/components/Sidebar";
import { useAuth } from "./hooks/useAuth";
import SideBar from "./nav/SideBar";



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
        <MsalPageProvider>
          <ReduxProvider>
            <div className="flex flex-col h-screen ">
              <Navbar />
              <div className="flex flex-1">
                <SideBar />
                <main className="flex-1 p-4 bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50">
                  {children}
                </main>
              </div>
            </div>
          </ReduxProvider>
        </MsalPageProvider>

      </body>

    </html>

  );
}
