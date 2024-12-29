'use client'
import React, { useEffect } from "react";
import { useInitializeUser } from "../hooks/useInitializeUser";
import { useAuth } from "../hooks/useAuth";
import { useDispatch } from "react-redux";
import { fetchShoppingListData, getFrequentyBoughtItems } from "../lib/fetchShoppingLists";
import { getFrequentItems, setCurrentShoppingList, setShoppingLists } from "../redux/shoppingListSlice";
import { fetchCatalogData } from "../lib/fetchCatalog";
import { setCategories } from "../redux/catalogSlice";

export default function AppInitializer({ children }: { children: React.ReactNode }) {
    useInitializeUser();
    const { acquireToken, isAuthenticated } = useAuth();
    const dispatch = useDispatch()

    // Fetch Catalog Data and Shopping list data
    useEffect(() => {
        if (!isAuthenticated) {
            return;
        }
        async function fetchData() {

            // make sure there is a valid token in the storage
            await acquireToken();
            // Fetch categories
            const fetchedCategories = await fetchCatalogData();

            if (fetchedCategories) {
                dispatch(setCategories(fetchedCategories));
            }
            // Fetch shopping lists
            const fetchedShoppingLists = await fetchShoppingListData();

            if (fetchedShoppingLists) {
                dispatch(setShoppingLists(fetchedShoppingLists));
                dispatch(setCurrentShoppingList(fetchedShoppingLists[0]));
            }

            const frequentItems = await getFrequentyBoughtItems();
            if (frequentItems) {
                dispatch(getFrequentItems(frequentItems));
            }
        }
        fetchData();

    }, [isAuthenticated, acquireToken, dispatch]);

    return <>{children}</>;
}
