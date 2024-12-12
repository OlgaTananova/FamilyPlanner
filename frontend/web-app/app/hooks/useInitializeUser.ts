import { useEffect } from "react";
import { useMsal } from "@azure/msal-react";
import getIdToken from "../lib/getIdToken";
import { useDispatch } from "react-redux";
import { clearUser, setUser } from "../redux/userSlice";
import decodeJwt from "../lib/decodeJwt";
import toast from "react-hot-toast";

export const useInitializeUser = () => {
    const { instance } = useMsal();
    const dispatch = useDispatch();

    useEffect(() => {
        const initializeUser = async () => {
            const idToken = getIdToken();

            if (idToken) {
                const decodedToken = decodeJwt(idToken);
                const user = {
                    givenName: decodedToken.given_name || "",
                    family: decodedToken.extension_Family || "",
                    role: decodedToken.extension_Role || "",
                    email: decodedToken.emails ? decodedToken.emails[0] : "",
                };
                dispatch(setUser(user));
            } else {
                dispatch(clearUser());
            }
        };

        const handleRedirect = async () => {
            try {
                const response = await instance.handleRedirectPromise();
                if (response && response.idToken) {
                    initializeUser();
                }
            } catch (error) {
                console.error("Error handling redirect:", error);
                toast.error("Error handling redirect.");
            }
        };

        initializeUser();
        handleRedirect();
    }, [instance, dispatch]);
};
