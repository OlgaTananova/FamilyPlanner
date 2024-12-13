'use client'
import React, { useEffect } from "react";
import { useInitializeUser } from "../hooks/useInitializeUser";
import { useAuth } from "../hooks/useAuth";
import { useDispatch } from "react-redux";
import { fetchShoppingListData } from "../lib/fetchShoppingLists";
import { setCurrentShoppingList, setShoppingLists } from "../redux/shoppingListSlice";
import { fetchCatalogData } from "../lib/fetchCatalog";
import { setCategories } from "../redux/catalogSlice";

export default function AppInitializer({ children }: { children: React.ReactNode }) {
    useInitializeUser();
    const { acquireToken } = useAuth();
    const dispatch = useDispatch()

    // Fetch Catalog Data and Shopping list data
    useEffect(() => {
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
        }
        fetchData();

    }, []);

    return <>{children}</>;
}
