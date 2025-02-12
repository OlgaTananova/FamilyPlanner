'use client'
import * as signalR from "@microsoft/signalr";
import React, { createContext, useContext, useEffect, useState } from "react";
import toast from "react-hot-toast";
import { useDispatch } from "react-redux";
import { useAuth } from "../hooks/useAuth";
import { addCategory, addItem, Category, removeCategoryFromStore, removeItemFromStore, updateCategoryInStore, updateItemInStore } from "../redux/catalogSlice";
import { addShoppingList, deleteCatalogItemFromShoppingList, deleteShoppingListFromStore, deleteShoppingListItemFromStore, updateCatalogCategory, updateCatalogItem, updateShoppingListInStore } from "../redux/shoppingListSlice";

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
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState(false);
    const dispatch = useDispatch();

    const establishConnection = async () => {
        try {
            // Prevent multiple connection attempts
            if (connection?.state === signalR.HubConnectionState.Connected) {
                console.warn("SignalR connection already established.");
                return;
            }
            // Acquire access token
            const token = await acquireToken();
            if (!token) {
                console.error("Failed to acquire token.");
                return;
            }

            // Ensure previous connection is stopped before creating a new one
            if (connection) {
                await connection.stop();
                console.warn("Existing SignalR connection stopped before re-establishing.");
            }

            const newConnection = new signalR.HubConnectionBuilder()
                .withUrl(`${hubUrl}`, {
                    accessTokenFactory: () => token.accessToken || "",
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
                    withCredentials: false,
                    headers: {
                        Authorization: `Bearer ${token.accessToken}`
                    }
                })
                .withAutomaticReconnect([0, 2000, 5000, 10000])
                .configureLogging(signalR.LogLevel.Information)
                .build();

            newConnection.serverTimeoutInMilliseconds = 60000; // 60 seconds (Default is 30 seconds)
            newConnection.keepAliveIntervalInMilliseconds = 20000;
            
            newConnection.onreconnecting(() => {
                console.warn("SignalR reconnecting...");
            });
            newConnection.onreconnected(() => {
                console.log("SignalR reconnected successfully!");
                setIsConnected(true);
            });
            newConnection.onclose(() => {
                console.error("SignalR connection closed. Please try to refresh the page.");
                //toast.error("Real-time notification service is disconnected. Please try to refresh the page.");
                setIsConnected(false);
            });


            await newConnection.start();
            console.log("SignalR connected!");
            setConnection(newConnection);
            setIsConnected(true);
        } catch (error) {
            console.error("SignalR connection error:", error);
            setIsConnected(false);
        }
    };

    useEffect(() => {
        if (!isAuthenticated) {
            // Disconnect SignalR if the user logs out
            if (connection) {
                connection.stop().catch((error) => console.error("Error stopping SignalR:", error));
            }
            setConnection(null);
            setIsConnected(false);
            return;
        }

        // Establish SignalR connection
        establishConnection();

        // Cleanup on unmount
        return () => {
            if (connection) {
                connection.stop().catch((error) => console.error("Error stopping SignalR on cleanup:", error));
            }
        };
    }, [isAuthenticated, hubUrl]);

    useEffect(() => {
        if (connection && isConnected) {
            // Catalog Events
            connection.on("CatalogCategoryCreated", (category: Category) => {
                dispatch(addCategory(category));
            });
            connection.on("CatalogCategoryDeleted", (deletedCategory: Category) => {
                dispatch(removeCategoryFromStore(deletedCategory.sku));
            });
            connection.on("CatalogCategoryUpdated", (updatedCategory: Category) => {
                dispatch(updateCategoryInStore(updatedCategory));
                dispatch(updateCatalogCategory(updatedCategory));
            });
            connection.on("CatalogItemUpdated", (updatedItem) => {
                dispatch(updateItemInStore(updatedItem));
                dispatch(updateCatalogItem(updatedItem.updatedItem));
            });
            connection.on("CatalogItemCreated", (createdItem) => {
                dispatch(addItem(createdItem));
            });
            connection.on("CatalogItemDeleted", (deletedItem) => {
                dispatch(removeItemFromStore(deletedItem.sku));
                dispatch(deleteCatalogItemFromShoppingList(deletedItem.sku));
            });
            // Shopping List Events
            connection.on("ShoppingListCreated", (shoppingList) => {
                dispatch(addShoppingList(shoppingList));
            });
            connection.on("ShoppingListDeleted", (shoppingList: { id: string, family: string, ownerId: string }) => {
                dispatch(deleteShoppingListFromStore(shoppingList.id))
            });
            connection.on("ShoppingListUpdated", (shoppingList) => {
                dispatch(updateShoppingListInStore(shoppingList));
            });

            connection.on("ShoppingListItemUpdated", (updatedShoppingList) => {
                dispatch(updateShoppingListInStore(updatedShoppingList));
            });

            connection.on("ShoppingListItemsAdded", (updatedShoppingList) => {
                dispatch(updateShoppingListInStore(updatedShoppingList));
            });
            connection.on("ShoppingListItemDeleted", (data: { shoppingListId: string, itemId: string, ownerId: string, family: string }) => {
                dispatch(deleteShoppingListItemFromStore({ shoppingListId: data.shoppingListId, itemId: data.itemId }));
            });

            return () => {
                connection.off("CatalogItemUpdated");
                connection.off("CatalogItemCreated");
                connection.off("CatalogItemDeleted");

                connection.off("CatalogCategoryCreated");
                connection.off("CatalogCategoryDeleted");
                connection.off("CatalogCategoryUpdated");

                connection.off("ShoppingListCreated");
                connection.off("ShoppingListDeleted");
                connection.off("ShoppingListUpdated");

                connection.off("ShoppingListItemUpdated");
                connection.off("ShoppingListItemsAdded");
                connection.off("ShoppingListItemDeleted");
            };
        }
    }, [connection, isConnected, dispatch]);

    return (
        <SignalRContext.Provider value={{ connection, isConnected }}>
            {children}
        </SignalRContext.Provider>
    );
};
