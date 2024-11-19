// Reusable Auth Button
'use client'
import { Button } from "flowbite-react";

interface AuthButtonProps {
    isAuthenticated: boolean,
    signOut: () => void,
    signIn: () => void
}

export default function AuthButton({ isAuthenticated, signOut, signIn }: AuthButtonProps) {
    return (<Button
        color={isAuthenticated ? "light" : "purple"}
        onClick={isAuthenticated ? signOut : signIn}
        className={`transition-all duration-200 ${isAuthenticated
            ? "hover:bg-purple-500 hover:text-white"
            : "bg-purple-400 text-white hover:bg-fuchsia-500"
            }`}
    >
        {isAuthenticated ? "Logout" : "Sign In"}
    </Button>)
};