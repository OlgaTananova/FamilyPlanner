'use client'
import React, { useEffect } from "react";
import { useAuth } from "../hooks/useAuth";
import { useDispatch } from "react-redux";
import { useShoppingListApi } from "../hooks/useShoppingListApi";
import { getFrequentItems, setCurrentShoppingList, setShoppingLists } from "../redux/shoppingListSlice";
import { useCatalogApi } from "../hooks/useCatalogApi";
import { setCategories } from "../redux/catalogSlice";
import { setUser } from "../redux/userSlice";

export default function AppInitializer({ children }: { children: React.ReactNode }) {
    const { acquireToken, isAuthenticated, instance } = useAuth();
    const dispatch = useDispatch()
    const { fetchShoppingListData, getFrequentyBoughtItems } = useShoppingListApi();
    const { fetchCatalogData } = useCatalogApi();

    // Fetch Catalog Data and Shopping list data
    useEffect(() => {
        if (!isAuthenticated) {
            return;
        }
        async function fetchData() {

            // retrieve the valid token
            const token = await acquireToken();

            // set the user 
            const claims = token?.idTokenClaims;
            if (claims) {

                const user = {
                    givenName: claims?.given_name || "",
                    family: claims?.extension_Family || "",
                    role: claims?.extension_Role || "",
                    email: claims?.emails[0] || "",
                };
                dispatch(setUser(user));
            }

            const [fetchedCategories, fetchedShoppingLists, frequentItems] = await Promise.all([fetchCatalogData(), fetchShoppingListData(), getFrequentyBoughtItems()]);
            // Fetch categories

            if (fetchedCategories) {
                dispatch(setCategories(fetchedCategories));
            }

            if (fetchedShoppingLists) {
                dispatch(setShoppingLists(fetchedShoppingLists));
                dispatch(setCurrentShoppingList(fetchedShoppingLists[0]));
            }
            if (frequentItems) {
                dispatch(getFrequentItems(frequentItems));
            }
        }
        fetchData();

    }, [isAuthenticated]);

    useEffect(() => {
        const callbackId = instance.addEventCallback((event) => {
            if (event.eventType === "msal:loginSuccess") {
                console.log("Login success, reloading window...");
                window.location.reload();
            }
        });

        return () => {
            if (callbackId) {
                instance.removeEventCallback(callbackId);
            }
        };
    }, [instance]);

    return <>{children}</>;
}
