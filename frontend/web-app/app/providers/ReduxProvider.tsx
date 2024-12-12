"use client";

import { Provider } from "react-redux";
import { store } from "../redux/store";
import { useMsal } from "@azure/msal-react";
import { useEffect } from "react";
import getIdToken from "../lib/getIdToken";
import { clearUser, setUser } from "../redux/userSlice";
import decodeJwt from "../lib/decodeJwt";
import toast from "react-hot-toast";

export default function ReduxProvider({ children }: { children: React.ReactNode }) {

  // const { instance } = useMsal();

  // useEffect(() => {
  //   // Initialize user data on app load
  //   const initializeUser = async () => {
  //     const idToken = getIdToken();

  //     if (idToken) {

  //       const decodedToken = decodeJwt(idToken);
  //       const user = {
  //         givenName: decodedToken.given_name || "",
  //         family: decodedToken.extension_Family || "",
  //         role: decodedToken.extension_Role || "",
  //         email: decodedToken.emails ? decodedToken.emails[0] : "",
  //       }
  //       store.dispatch(setUser(user));

  //     } else {
  //       store.dispatch(clearUser());
  //     }
  //   };

  //   const handleRedirect = async () => {
  //     try {

  //       const response = await instance.handleRedirectPromise();

  //       if (response && response.idToken) {
  //         initializeUser();
  //         return;
  //       }
  //     } catch (error) {
  //       console.error("Error handling redirect:", error);
  //       toast.error("Error handling redirect.")
  //     }

  //   };

  //   initializeUser();
  //   handleRedirect();
  // }, [instance]);
  return <Provider store={store}>{children}</Provider>;
}
