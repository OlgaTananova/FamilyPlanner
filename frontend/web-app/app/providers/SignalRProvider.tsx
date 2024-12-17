'use client'
import React, { createContext, useContext, useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { useMsal } from "@azure/msal-react";
import { useAuth } from "../hooks/useAuth";
import { getAccessToken } from "../lib/getAccessToken";
import getIdToken from "../lib/getIdToken";
import { useDispatch, useSelector } from "react-redux";
import { updateCatalogItem } from "../redux/shoppingListSlice";
import { RootState } from "../redux/store";
import { addItem, removeItemFromStore, updateItemInStore } from "../redux/catalogSlice";

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
    const items = useSelector((state: RootState) => state.categories.itemsWOCategories);

    const establishConnection = async () => {
        try {
            // Acquire access token
            const token = await acquireToken();
            if (!token) {
                console.error("Failed to acquire token.");
                return;
            }

            // Build and start a SignalR connection
            const newConnection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl, {
                    //accessTokenFactory: () => token.accessToken,
                    withCredentials: true,
                    headers: {
                        Authorization: `Bearer ${token.accessToken}`,
                    }
                })
                .withAutomaticReconnect()
                .build();

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
                connection.stop();
                console.log("SignalR disconnected due to logout.");
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
                connection.stop();
                console.log("SignalR connection closed.");
            }
        };
    }, [isAuthenticated, hubUrl]);

    useEffect(() => {
        if (connection && isConnected) {
            connection.on("CatalogItemUpdated", (updatedItem) => {
                dispatch(updateItemInStore(updatedItem));
                dispatch(updateCatalogItem(updatedItem.updatedItem));
            });
            connection.on("CatalogItemCreated", (createdItem) => {
                console.log(createdItem);
                dispatch(addItem(createdItem));
            });
            connection.on("CatalogItemDeleted", (deletedItem) => {
                console.log(deletedItem);
                dispatch(removeItemFromStore(deletedItem.sku))
            })

            return () => {
                connection.off("CatalogItemUpdated");
                connection.off("CatalogItemCreated");
            };
        }
    }, [connection, isConnected]);

    return (
        <SignalRContext.Provider value={{ connection, isConnected }}>
            {children}
        </SignalRContext.Provider>
    );
};