"use client";
import { useMsal } from "@azure/msal-react";
import { Button } from "flowbite-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import toast from "react-hot-toast";
import { useDispatch, useSelector } from "react-redux";
import { useAuth } from "../hooks/useAuth";
import { RootState } from "../redux/store";
import { clearUser, setUser } from "../redux/userSlice";

function UserProfile() {
  const { isAuthenticated, editProfile, acquireToken } = useAuth();
  const { instance } = useMsal();
  const user = useSelector((state: RootState) => state.user);
  const dispatch = useDispatch();
  const router = useRouter();


  const updateUserProfile = async () => {
    const claims = (await acquireToken()).idTokenClaims;
    if (claims) {

      const user = {
        givenName: claims?.given_name || "",
        family: claims?.extension_Family || "",
        role: claims?.extension_Role || "",
        email: claims?.emails[0] || "",
      };
      dispatch(setUser(user));
    }
  };


  useEffect(() => {
    // Redirect to login page if the user is not authenticated
    if (!isAuthenticated) {
      router.push("/");
      dispatch(clearUser());
      return;
    };

    // Handle redirect and fetch updated profile data
    const handleRedirect = async () => {
      try {
        const response = await instance.handleRedirectPromise();

        if (response && response.idToken && response.authority.includes("edit_profile")) {
          updateUserProfile();
          toast.success("User profile was successfully updated.")
          return;
        }
      } catch (error) {
        console.error("Error handling redirect:", error);
        toast.error("Error handling redirect.")
      }

    };

    handleRedirect();
    // Fetch the initial profile data
    updateUserProfile();
  }, [isAuthenticated, instance, router, dispatch]);

  // Handle profile editing flow
  async function handleEditProfile() {
    try {
      await editProfile();
    } catch (error) {
      console.error("Error initiating edit profile:", error);
      toast.error("Failed to initiate profile edit.");
    }
  };

  if (!isAuthenticated && !user) {
    return (
      <div className="text-m h-screen text-black-200 mb-6 text-center pt-4">The user is not authenticated.</div>
    )
  }


  return (
    <div className="flex justify-center items-start h-screen bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50">
      <div className="mt-6 p-8 bg-white rounded-lg shadow-lg max-w-md w-full relative">
        <Link href={("/")}
          className="absolute top-2 right-2 text-gray-600 hover:text-gray-900"
          aria-label="Close Profile"
        >
          ✖
        </Link>
        <h1 className="text-2xl font-bold text-purple-700 mb-6 text-center">User Profile</h1>
        <div className="text-gray-700 text-sm sm:text-base">
          <p className="mb-4">
            <strong>Given Name:</strong> {user?.givenName || ""}
          </p>
          <p className="mb-4">
            <strong>Family Name:</strong> {user?.family || ""}
          </p>
          <p className="mb-4">
            <strong>Role:</strong> {user?.role || ""}
          </p>
          <p className="mb-4">
            <strong>Email:</strong> {user?.email || ""}
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
