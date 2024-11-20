'use client'
import { useState } from "react";
import { MdFamilyRestroom } from "react-icons/md";
import { Button } from "flowbite-react";
import { useAuth } from "../hooks/useAuth";
import AuthButton from "./AuthButton";
import Link from "next/link";
import { useRouter, usePathname } from "next/navigation";

export default function Navbar() {
  const auth = useAuth();
  const { isAuthenticated, account, signIn, signOut } = auth;
  // TODO: replace with a userProfile info from the store;
  const userName = account?.idTokenClaims?.given_name[0] ?? "N/A";
  const router = useRouter();
  const path = usePathname();

  const handldeProfileIconClick = () => {
    if (path.includes("profile")){
      router.push("/");
    } else {
      router.push("/profile");
    }
  }

  return (
    <nav
      className="bg-gradient-to-r from-purple-100 via-purple-50 to-fuchsia-100 
                 shadow-md p-4 flex justify-between items-center w-full 
                 md:px-6 lg:px-12"
    >
      {/* Logo and Header */}
      <div className="flex items-center space-x-3">
        <MdFamilyRestroom size={36} className="text-purple-500" />
        <h1 className="text-lg font-semibold text-purple-600 md:text-xl lg:text-2xl">
          Family Planner
        </h1>
      </div>

      {/* Right-side Actions */}
      <div className="flex items-center space-x-4">
        {isAuthenticated ? (
          <>
            <Link href={"/profile"} onClick={handldeProfileIconClick}
              className="w-8 h-8 flex items-center justify-center rounded-full 
                         bg-purple-500 text-white text-sm md:w-10 md:h-10 md:text-base 
                         shadow-lg"
            >
              {userName}
            </Link>
            <AuthButton isAuthenticated={isAuthenticated} signIn={signIn} signOut={signOut} />
          </>
        ) : (
          <AuthButton isAuthenticated={isAuthenticated} signIn={signIn} signOut={signOut} />)
        }
      </div>
    </nav>
  );
}
