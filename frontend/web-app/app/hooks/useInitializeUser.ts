import { useMsal } from "@azure/msal-react";
import { useEffect } from "react";
import { useDispatch } from "react-redux";
import decodeJwt from "../lib/decodeJwt";
import getIdToken from "../lib/getIdToken";
import { clearUser, setUser } from "../redux/userSlice";

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
                    id: decodedToken.oid ? decodedToken.oid : ""
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
            }
        };

        initializeUser();
        handleRedirect();
    }, [instance, dispatch]);
};
