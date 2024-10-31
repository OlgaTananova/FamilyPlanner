'use client'
import Link from 'next/link';
import React, { useState } from 'react'
import { FiMenu, FiX } from 'react-icons/fi';
import { MdFamilyRestroom } from "react-icons/md";

export default function Navbar() {
    const [menuOpen, setMenuOpen] = useState(false);

    const toggleMenu = () => {
      setMenuOpen(!menuOpen);
    };
  return (
    <header className="bg-gradient-to-r from-blue-100 via-green-100 to-purple-100 shadow-md">
    <div className="max-w-[480px] sm:max-w-[768px] lg:max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8">
      <div className="flex items-center justify-between h-16">
        {/* Logo and Title */}
        <div className="flex items-center space-x-2">
          <MdFamilyRestroom size={36} className="text-blue-500" />
          <h1 className="text-lg sm:text-xl font-semibold text-blue-800">Family Planner</h1>
        </div>

        {/* Desktop Navigation Links */}
        <div className="hidden sm:flex items-center space-x-4">
          <button className="px-4 py-1 text-sm font-medium text-white bg-green-500 rounded-md hover:bg-green-400">
            Sign Up
          </button>
          <button className="px-4 py-1 text-sm font-medium text-white bg-blue-500 rounded-md hover:bg-blue-400">
            Sign In
          </button>
        </div>

        {/* Mobile Menu Icon */}
        <button
          onClick={toggleMenu}
          className="sm:hidden flex items-center justify-center text-blue-600 focus:outline-none"
        >
          {menuOpen ? <FiX size={24} /> : <FiMenu size={24} />}
        </button>
      </div>
    </div>

    {/* Mobile Menu */}
    {menuOpen && (
      <div className="sm:hidden bg-gradient-to-r from-blue-50 via-green-50 to-purple-50 shadow-md">
        <div className="px-4 py-2">
          <Link href="" className="w-full text-left py-2 text-blue-700 font-medium hover:text-green-600">
            Sign Up
          </Link>
          <button className="w-full text-left py-2 text-blue-700 font-medium hover:text-green-600">
            Sign In
          </button>
        </div>
      </div>
    )}
  </header>
  )
}
