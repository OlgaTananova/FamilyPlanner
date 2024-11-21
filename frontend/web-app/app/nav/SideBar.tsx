'use client'
import { useState } from "react";
import { FaUsers, FaListUl, FaShoppingCart, FaMoneyBillWave } from "react-icons/fa";
import { HiChevronLeft, HiChevronRight } from "react-icons/hi";
import Link from "next/link";

export default function SideBar() {
  const [isCollapsed, setIsCollapsed] = useState(false);

  const toggleSidebar = () => setIsCollapsed(!isCollapsed);

  return (
    <aside
      className={`bg-gradient-to-b from-purple-100 via-purple-50 to-fuchsia-100 
                  h-screen p-4 shadow-md transition-all duration-300 
                  ${isCollapsed ? "w-16" : "w-56"} 
                  flex flex-col justify-between`}
    >
      {/* Top Section: Logo and Links */}
      <div>
        {/* Logo */}
        <div className="flex items-center justify-center mb-6">
        </div>

        {/* Navigation Links */}
        <nav className="space-y-4">
          <SidebarLink
            href="/family"
            icon={<FaUsers size={24} />}
            label="Family"
            isCollapsed={isCollapsed}
          />
          <SidebarLink
            href="/catalog"
            icon={<FaListUl size={24} />}
            label="Catalog"
            isCollapsed={isCollapsed}
          />
          <SidebarLink
            href="/shopping-list"
            icon={<FaShoppingCart size={24} />}
            label="Shopping List"
            isCollapsed={isCollapsed}
          />
          <SidebarLink
            href="/budget"
            icon={<FaMoneyBillWave size={24} />}
            label="Budget"
            isCollapsed={isCollapsed}
          />
        </nav>
      </div>

      {/* Bottom Section: Collapse Button */}
      <div
        className="flex items-center justify-center cursor-pointer text-purple-500"
        onClick={toggleSidebar}
      >
        {isCollapsed ? <HiChevronRight size={24} /> : <HiChevronLeft size={24} />}
      </div>
    </aside>
  );
}

// Sidebar Link Component
function SidebarLink({ href, icon, label, isCollapsed }: any) {
  return (
    <Link href={href}
        className={`flex items-center space-x-3 p-2 rounded-md transition-all 
                    hover:bg-purple-200 text-purple-600 
                    ${isCollapsed ? "justify-center" : ""}`}
      >
        {icon}
        {!isCollapsed && <span className="text-md font-medium">{label}</span>}
    </Link>
  );
}
