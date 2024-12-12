'use client'
import React from "react";
import { useInitializeUser } from "../hooks/useInitializeUser";

export default function AppInitializer({ children }: { children: React.ReactNode }) {
    useInitializeUser();

    return <>{children}</>;
}
