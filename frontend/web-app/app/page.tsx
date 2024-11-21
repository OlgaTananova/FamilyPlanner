'use client'
import Image from "next/image";
import { useAuth } from "./hooks/useAuth";
import { useEffect, useState } from "react";
import ProfilePage from "./profile/page";
import { useRouter } from "next/navigation";
import { Button } from "flowbite-react";
import DashboardPage from "./dashboard/page";
import getIdToken from "./lib/getIdToken";
import decodeJwt from "./lib/decodeJwt";
import { useDispatch } from "react-redux";
import { clearUser, setUser } from "./redux/userSlice";
import SideBar from "./nav/SideBar";

export default function Home() {

  const { isAuthenticated } = useAuth();
  const router = useRouter();
  const dispatch = useDispatch();

  useEffect(() => {
    if (isAuthenticated) {
      const idToken = getIdToken();
      if (idToken) {
        const decodedToken = decodeJwt(idToken);
        const user = {
          givenName: decodedToken.given_name || "N/A",
          family: decodedToken.extension_Family || "N/A",
          role: decodedToken.extension_Role || "N/A",
          email: decodedToken.emails ? decodedToken.emails[0] : "N/A",
        }
        dispatch(setUser(user));
      }
    } else {
      dispatch(clearUser());
    }

  }, [isAuthenticated])

  if (isAuthenticated) {
    return (
      <div className="flex justify-between items-center h-screen bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50">
        <SideBar />
        <div className="p-8 bg-white rounded-lg shadow-lg max-w-md w-full text-center">
          <DashboardPage />
        </div>
      </div>
    );
  }

  return (
    <div className="flex justify-center items-center h-screen bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50">
      <div className="p-8 bg-white rounded-lg shadow-lg max-w-md w-full text-center">
        <h1 className="text-3xl font-bold text-purple-700 mb-6">
          Welcome to Family Planner
        </h1>
        <p className="text-gray-700 mb-6">
          Organize your family life with ease. Sign in to get started!
        </p>
      </div>
    </div>
  );
}
