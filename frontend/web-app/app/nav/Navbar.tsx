'use client'
import Link from 'next/link';
import React, { useState } from 'react'
import { FiMenu, FiX } from 'react-icons/fi';
import { MdFamilyRestroom } from "react-icons/md";
import LoginButton from './LoginButton';
import { useAuth } from '../hooks/useAuth';
import { Button } from 'flowbite-react';


export default function Navbar () {
  const auth = useAuth();
  const {isAuthenticated, account, signIn, signOut} = auth;
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const userName = 'A'; // Placeholder, can be dynamic based on user data

  return (
    <nav className="bg-white shadow-md p-4 flex justify-between items-center w-full">
      {/* Logo and Header */}
      <div className="flex items-center space-x-3">
        <MdFamilyRestroom size={40} color='blue'></MdFamilyRestroom>
        <h1 className="text-lg font-semibold text-blue-600">Family Planner</h1>
      </div>

      {/* Right-side Actions */}
      <div className="flex items-center space-x-4">
        {isAuthenticated ? (
          <>
            <div className="w-8 h-8 flex items-center justify-center rounded-full bg-green-500 text-white text-sm">
              {userName}
            </div>
            <Button
              color="light"
              onClick={() => signOut()}
              className="hover:bg-blue-500 hover:text-white"
            >
              Logout
            </Button>
          </>
        ) : (
          <Button
            color="blue"
            onClick={() => signIn()}
            className="text-white"
          >
            Sign In
          </Button>
        )}
      </div>
    </nav>
  );
};

