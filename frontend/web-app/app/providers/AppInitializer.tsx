'use client'
import React, { useEffect, useState } from "react";
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

    const [isAppReady, setIsAppReady] = useState(false);
    const [error, setError] = useState<string | null>(null)

    // Fetch Catalog Data and Shopping list data
    useEffect(() => {
        if (!isAuthenticated) {
            return;
        }
        const fetchData = async () => {
            try {
                const token = await acquireToken();
                if (!token || !token.accessToken) {
                    console.error("Token acquisition failed.");
                    setError("Authentication failed.");
                    return;
                }

                const claims = token.idTokenClaims;
                if (claims) {
                    dispatch(setUser({
                        givenName: claims.given_name || "",
                        family: claims.extension_Family || "",
                        role: claims.extension_Role || "",
                        email: claims.emails?.[0] || "",
                    }));
                }

                const [fetchedCategories, fetchedShoppingLists, frequentItems] = await Promise.all([
                    fetchCatalogData(),
                    fetchShoppingListData(),
                    getFrequentyBoughtItems(),
                ]);

                if (fetchedCategories) dispatch(setCategories(fetchedCategories));
                if (fetchedShoppingLists) {
                    dispatch(setShoppingLists(fetchedShoppingLists));
                    dispatch(setCurrentShoppingList(fetchedShoppingLists[0]));
                }
                if (frequentItems) dispatch(getFrequentItems(frequentItems));

                setIsAppReady(true);
            } catch (err: any) {
                console.error("App initialization error:", err);
                setError("Failed to load app data.");
            }
        };

        fetchData();
    }, [isAuthenticated]);


    useEffect(() => {
        const callbackId = instance.addEventCallback((event) => {
            if (event.eventType === "msal:loginSuccess") {
                console.log("Login success — reloading app data.");
                setIsAppReady(false); 
            }
        });

        return () => {
            if (callbackId) {
                instance.removeEventCallback(callbackId);
            }
        };
    }, [instance]);

    if (!isAppReady && isAuthenticated) {
        return (
            <div className="flex items-center justify-center h-screen bg-white">
                <div className="text-center">
                    <div className="animate-spin h-12 w-12 border-4 border-purple-500 border-t-transparent rounded-full mx-auto" />
                    <p className="mt-4 text-lg font-semibold">Loading your data...</p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex items-center justify-center h-screen bg-red-50">
                <div className="text-center text-red-800">
                    <p className="text-lg font-semibold">Oops! {error}</p>
                    <p className="text-sm mt-2">Try refreshing the page or logging in again.</p>
                </div>
            </div>
        );
    }

    return <>{children}</>;
}
