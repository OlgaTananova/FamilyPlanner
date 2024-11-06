'use client'
import React from 'react'
import { InteractionType, RedirectRequest } from "@azure/msal-browser";
import { useAuth } from '../hooks/useAuth';

const loginRequest: RedirectRequest = {
  scopes: ["openid", "profile", "offline_access"],
};
export default function LoginButton() {

  const { signIn, signOut, isAuthenticated, account } = useAuth();

  return (
    <button onClick={signIn} className="px-4 py-1 text-sm font-medium text-white bg-green-500 rounded-md hover:bg-green-400">
    Sign In
  </button>
  )
}
