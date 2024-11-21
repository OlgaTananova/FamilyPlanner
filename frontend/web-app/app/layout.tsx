import type { Metadata } from "next";
import "./globals.css";
import Navbar from "./nav/Navbar";
import MsalPageProvider from "./providers/MsalPageProvider";
import ToasterProvider from "./providers/ToasterProvider";
import ReduxProvider from "./providers/ReduxProvider";



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
        <MsalPageProvider>
          <ReduxProvider>
            <>
              <Navbar />
            <main>
              {children}
            </main>
            </>
          </ReduxProvider>
        </MsalPageProvider>

      </body>

    </html>

  );
}
