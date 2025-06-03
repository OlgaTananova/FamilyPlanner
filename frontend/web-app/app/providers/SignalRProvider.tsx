"use client";

import * as signalR from "@microsoft/signalr";
import React, { createContext, useContext, useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { useAuth } from "../hooks/useAuth";
import {
    addCategory, addItem, removeCategoryFromStore, removeItemFromStore,
    updateCategoryInStore, updateItemInStore
} from "../redux/catalogSlice";
import {
    addShoppingList, deleteCatalogItemFromShoppingList, deleteShoppingListFromStore,
    deleteShoppingListItemFromStore, updateCatalogCategory, updateCatalogItem,
    updateShoppingListInStore
} from "../redux/shoppingListSlice";
import { RootState } from "../redux/store";

interface SignalRContextType {
    connection: signalR.HubConnection | null;
    isConnected: boolean;
}

const SignalRContext = createContext<SignalRContextType>({
    connection: null,
    isConnected: false,
});

export const useSignalR = () => useContext(SignalRContext);

interface SignalRProviderProps {
    hubUrl: string;
    children: React.ReactNode;
}

export const SignalRProvider: React.FC<SignalRProviderProps> = ({ hubUrl, children }) => {
    const { acquireToken, isAuthenticated } = useAuth();
    const user = useSelector((state: RootState) => state.user);
    const dispatch = useDispatch();

    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<signalR.HubConnection | null>(null);

    const setupSignalRHandlers = (conn: signalR.HubConnection) => {
        conn.on("CatalogCategoryCreated", (category) => dispatch(addCategory(category)));
        conn.on("CatalogCategoryDeleted", (deletedCategory) => dispatch(removeCategoryFromStore(deletedCategory.sku)));
        conn.on("CatalogCategoryUpdated", (updatedCategory) => {
            dispatch(updateCategoryInStore(updatedCategory));
            dispatch(updateCatalogCategory(updatedCategory));
        });

        conn.on("CatalogItemCreated", (createdItem) => dispatch(addItem(createdItem)));
        conn.on("CatalogItemUpdated", (updatedItem) => {
            dispatch(updateItemInStore(updatedItem));
            dispatch(updateCatalogItem(updatedItem.updatedItem));
        });
        conn.on("CatalogItemDeleted", (deletedItem) => {
            dispatch(removeItemFromStore(deletedItem.sku));
            dispatch(deleteCatalogItemFromShoppingList(deletedItem.sku));
        });

        conn.on("ShoppingListCreated", (shoppingList) => dispatch(addShoppingList(shoppingList)));
        conn.on("ShoppingListUpdated", (shoppingList) => dispatch(updateShoppingListInStore(shoppingList)));
        conn.on("ShoppingListDeleted", (shoppingList) => dispatch(deleteShoppingListFromStore(shoppingList.id)));

        conn.on("ShoppingListItemUpdated", (shoppingList) => dispatch(updateShoppingListInStore(shoppingList)));
        conn.on("ShoppingListItemsAdded", (shoppingList) => dispatch(updateShoppingListInStore(shoppingList)));
        conn.on("ShoppingListItemDeleted", (data) => {
            dispatch(deleteShoppingListItemFromStore({
                shoppingListId: data.shoppingListId,
                itemId: data.itemId
            }));
        });
    };

    const establishConnection = async () => {
        try {
            const tokenResult = await acquireToken();
            if (!tokenResult?.accessToken) {
                console.error("Failed to acquire token for SignalR.");
                return;
            }

            if (connectionRef.current) {
                await connectionRef.current.stop();
                connectionRef.current = null;
            }

            const newConnection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl, {
                    accessTokenFactory: () => tokenResult.accessToken!,
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
                    withCredentials: false,
                })
                .withAutomaticReconnect([0, 2000, 5000, 10000])
                .configureLogging(signalR.LogLevel.Information)
                .build();

            newConnection.serverTimeoutInMilliseconds = 60000;
            newConnection.keepAliveIntervalInMilliseconds = 20000;

            newConnection.onreconnecting(() => {
                console.warn("SignalR reconnecting...");
                setIsConnected(false);
            });

            newConnection.onreconnected(() => {
                console.log("SignalR reconnected.");
                setIsConnected(true);
            });

            newConnection.onclose(() => {
                console.warn("SignalR connection closed.");
                setIsConnected(false);
            });

            setupSignalRHandlers(newConnection);

            await newConnection.start();
            connectionRef.current = newConnection;
            setConnection(newConnection);
            setIsConnected(true);
            console.log("SignalR connected.");
        } catch (error) {
            console.error("SignalR connection error:", error);
            setIsConnected(false);
        }
    };

    useEffect(() => {
        if (!isAuthenticated || !user?.email) return;

        establishConnection();

        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop().catch((err) =>
                    console.error("Error disconnecting SignalR:", err)
                );
            }
        };
    }, [isAuthenticated, user.family, hubUrl]);

    return (
        <SignalRContext.Provider value={{ connection, isConnected }}>
            {children}
        </SignalRContext.Provider>
    );
};