'use client'
import React, { createContext, useContext, useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { useMsal } from "@azure/msal-react";
import { useAuth } from "../hooks/useAuth";
import { getAccessToken } from "../lib/getAccessToken";
import getIdToken from "../lib/getIdToken";

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
    const { acquireToken } = useAuth();
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState(false);


    useEffect(() => {
        const token = getAccessToken();
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                withCredentials: true,
                headers: {
                    Authorization: `Bearer ${token || " "}`
                }
            }
            )
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);

        newConnection
            .start()
            .then(() => {
                console.log("SignalR connected!");
                setIsConnected(true);
            })
            .catch((err) => {
                console.error("SignalR connection error:", err);
                setIsConnected(false);
            });

        return () => {
            if (newConnection) {
                newConnection.stop();
                console.log("SignalR disconnected!");
            }
        };
    }, []);

    return (
        <SignalRContext.Provider value={{ connection, isConnected }}>
            {children}
        </SignalRContext.Provider>
    );
};
