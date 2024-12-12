'use client'
import { useEffect, useState } from "react";
import { MdFamilyRestroom } from "react-icons/md";
import { Button } from "flowbite-react";
import { useAuth } from "../hooks/useAuth";
import AuthButton from "./AuthButton";
import Link from "next/link";
import { useRouter, usePathname } from "next/navigation";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../redux/store";
import { clearCategories } from "../redux/catalogSlice";
import { clearUser } from "../redux/userSlice";

export default function Navbar() {
  const auth = useAuth();
  const { isAuthenticated, account, signIn, signOut } = auth;
  const user = useSelector((state: RootState) => state.user);
  const [userInitials, setUserInitials] = useState("");
  const router = useRouter();
  const path = usePathname();
  const dispatch = useDispatch();

  const handleSignOut = () => {
    signOut();
    dispatch(clearCategories());
    dispatch(clearUser());

  }

  useEffect(() => {
    setUserInitials(() => user?.givenName ? user.givenName[0] : "NA")
  }, [user])

  return (
    <nav
      className="bg-gradient-to-r from-purple-100 via-purple-50 to-fuchsia-100 
                 shadow-md p-4 flex justify-between items-center 
                 sticky top-0 z-50 w-full"
    >
      {/* Logo and Header */}
      <Link href={"/"} className="flex items-center space-x-3">
        <MdFamilyRestroom size={36} className="text-purple-500" />
        <h1 className="text-lg sm:text-sm font-semibold text-purple-600 md:text-xl lg:text-2xl">
          Family Planner
        </h1>
      </Link>

      {/* Right-side Actions */}
      <div className="flex items-center space-x-4">
        {isAuthenticated ? (
          <>
            <Link href={"/profile"}
              className="w-8 h-8 flex items-center justify-center rounded-full 
                         bg-purple-500 text-white text-sm md:w-10 md:h-10 md:text-base 
                         shadow-lg"
            >
              {userInitials}
            </Link>
            <AuthButton isAuthenticated={isAuthenticated} signIn={signIn} signOut={handleSignOut} />
          </>
        ) : (
          <AuthButton isAuthenticated={isAuthenticated} signIn={signIn} signOut={handleSignOut} />)
        }
      </div>
    </nav>
  );
}
