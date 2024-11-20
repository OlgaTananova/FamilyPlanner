"use client";

import { useAuth } from "../hooks/useAuth";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import decodeJwt from "../lib/decodeJwt";
import { Button } from "flowbite-react";
import getIdToken from "../lib/getIdToken";
import { useMsal } from "@azure/msal-react";

function UserProfile() {
  const { isAuthenticated, editProfile } = useAuth();
  const { instance } = useMsal();
  //TODO save the userstate information in a separate store
  const [userProfile, setUserProfile] = useState<any>(null);
  const router = useRouter();

  const updateUserProfile = () => {
    const idToken = getIdToken();
    if (idToken) {
      const decodedToken = decodeJwt(idToken);
      setUserProfile({
        givenName: decodedToken.given_name || "N/A",
        family: decodedToken.extension_Family || "N/A",
        role: decodedToken.extension_Role || "N/A",
        email: decodedToken.emails ? decodedToken.emails[0] : "N/A",
      });
    }
  };

  useEffect(() => {
    // Redirect to login page if the user is not authenticated
    if (!isAuthenticated) {
      router.push("/");
      return;
    }

    // Handle redirect and fetch updated profile data
    const handleRedirect = async () => {
      try {
        const response = await instance.handleRedirectPromise();
        if (response && response.idToken) {
          updateUserProfile();
        }
      } catch (error) {
        console.error("Error handling redirect:", error);
      }
    };

    handleRedirect();

    // Fetch the initial profile data
    updateUserProfile();
  }, [isAuthenticated, instance, router]);

  // Handle profile editing flow
  async function handleEditProfile() {

    await editProfile();
  };


  // TODO: 
  if (!isAuthenticated && !userProfile) {
    return (
      <div className="text-m h-screen text-black-200 mb-6 text-center pt-4">The user is not authenticated.</div>
    )
  } else if (!isAuthenticated && userProfile) {
    setUserProfile(null);
  }

  return (
    <div className="flex justify-center items-start h-screen bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50">
      <div className="mt-6 p-8 bg-white rounded-lg shadow-lg max-w-md w-full">
        <h1 className="text-2xl font-bold text-purple-700 mb-6 text-center">User Profile</h1>
        <div className="text-gray-700 text-sm sm:text-base">
          <p className="mb-4">
            <strong>Given Name:</strong> {userProfile?.givenName || "N/A"}
          </p>
          <p className="mb-4">
            <strong>Family Name:</strong> {userProfile?.family || "N/A"}
          </p>
          <p className="mb-4">
            <strong>Role:</strong> {userProfile?.role || "N/A"}
          </p>
          <p className="mb-4">
            <strong>Email:</strong> {userProfile?.email || "N/A"}
          </p>
        </div>
        <div className="flex justify-center mt-6">
          <Button
            color="purple"
            onClick={handleEditProfile}
            className="bg-purple-500 hover:bg-purple-600 text-white"
          >
            Edit Profile
          </Button>
        </div>
      </div>
    </div>
  );
}

export default UserProfile;
