import type { Metadata } from "next";
import "./globals.css";
import Navbar from "./nav/Navbar";


export const metadata: Metadata = {
  title: "Family Planner",
  description: "Generated by create next app",
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
        <Navbar />
        <main>
          {children}
        </main>

      </body>
    </html>
  );
}
