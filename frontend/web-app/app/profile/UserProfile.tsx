'use client'
import { useAuth } from "../hooks/useAuth";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import decodeJwt from "../lib/decodeJwt";
import { Button } from "flowbite-react";
import getIdToken from "../lib/getIdToken";

function UserProfile() {
    const { isAuthenticated } = useAuth();
    const [userProfile, setUserProfile] = useState<any>(null);
    const router = useRouter();

       useEffect(() => {
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
    }, []);

    useEffect(() => {
        // Redirect to login page if the user is not authenticated
        if (!isAuthenticated) {
            router.push("/");
        }
    }, [isAuthenticated, router]);

    // TODO: set-up edit profile flow
    const handleEditProfile = () => {
        const editProfileUrl = `https://${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.b2clogin.com/${process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_NAME}.onmicrosoft.com/${process.env.NEXT_PUBLIC_AZURE_AD_B2C_PROFILE_EDIT_FLOW}/oauth2/v2.0/authorize?client_id=${process.env.NEXT_PUBLIC_AZURE_AD_B2C_CLIENT_ID}&redirect_uri=${process.env.NEXT_PUBLIC_AZURE_AD_B2C_REDIRECT_URI}&response_type=code&scope=openid&state=edit_profile`;
        window.location.href = editProfileUrl;
    };
    

    if (!isAuthenticated || !userProfile) {
        return <p>Loading...</p>;
    }

    return (
        <div className="p-8 bg-white">
            <h1 className="text-2xl font-bold text-purple-700 mb-4">User Profile</h1>
            <div className="text-gray-700">
                <p className="mb-2">
                    <strong>Given Name:</strong> {userProfile.givenName}
                </p>
                <p className="mb-2">
                    <strong>Family:</strong> {userProfile.family}
                </p>
                <p className="mb-2">
                    <strong>Role:</strong> {userProfile.role}
                </p>
                <p className="mb-2">
                    <strong>Email:</strong> {userProfile.email}
                </p>
            </div>
            <Button
                color="purple"
                onClick={handleEditProfile}
                className="mt-6 bg-purple-500 hover:bg-purple-600 text-white"
            >
                Edit Profile
            </Button>
        </div>
    );
};

export default UserProfile;
