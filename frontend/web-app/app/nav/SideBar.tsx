'use client'
import { useState } from "react";
import { FaUsers, FaListUl, FaShoppingCart, FaMoneyBillWave } from "react-icons/fa";
import { HiChevronLeft, HiChevronRight } from "react-icons/hi";
import Link from "next/link";
import { useSelector } from "react-redux";
import { RootState } from "../redux/store";
import { useAuth } from "../hooks/useAuth";

export default function SideBar() {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const familyName = useSelector((state: RootState) => state.user.family)
  const auth = useAuth();
  const { isAuthenticated } = auth;

  const toggleSidebar = () => setIsCollapsed(!isCollapsed);

  if (!isAuthenticated) {
    return (<></>)
  } else {
    return (
      <div
        className={`bg-gradient-to-b from-purple-100 via-purple-50 to-fuchsia-100 
          shadow-md transition-all duration-300 sticky top-16 flex-col justify-items-center
          h-[calc(100vh-64px)] 
          ${isCollapsed ? "w-6 sm:w-16" : "w-25 sm:w-30 md:w-56"}`}
      >{/* Collapse Button */}
        <div
          className={`
            cursor-pointer bg-gradient-to-b from-purple-100 to-fuchsia-100  hover:bg-purple-400 
            rounded-md shadow-md z-50 
            transition-all duration-300 mt-1 justify-self-end
            ${isCollapsed ? "w-6 h-6 pt-0.5 sm:w-6 sm:h-6" : "w-8 h-8 pt-1.5 pl-1"}`}
          onClick={toggleSidebar}
        >
          {isCollapsed ? <HiChevronRight size={20} color="purple"/> : <HiChevronLeft size={20} color="purple" />}
        </div>
        {/* Top Section: Links */}
        <div className={`${isCollapsed ? "hidden sm:block" : "block"} space-y-4 p-2`}>

          <SidebarLink
            href={`/family/${familyName}`}
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
        </div>


      </div>
    );
  }


}

// Sidebar Link Component
function SidebarLink({ href, icon, label, isCollapsed }: any) {
  return (
    <Link href={href}

      className={`flex items-center p-2 rounded-md transition-all 
                    hover:bg-purple-200 text-purple-600 
                    ${isCollapsed ? "justify-center" : "space-x-3"}`}>
      {icon}
      {!isCollapsed && <span className="md:text-md md:font-medium sm:text-sm sm:font-medium">{label}</span>}
    </Link>
  );
}
