'use client';
import { useState } from "react";
import { FaUsers, FaListUl, FaShoppingCart, FaMoneyBillWave } from "react-icons/fa";
import { HiChevronLeft, HiChevronRight } from "react-icons/hi";
import Link from "next/link";
import { useSelector } from "react-redux";
import { RootState } from "../redux/store";
import { useAuth } from "../hooks/useAuth";

export default function SideBar() {
    const [isCollapsed, setIsCollapsed] = useState(false);
    const [isOpen, setIsOpen] = useState(false); // For mobile view
    const familyName = useSelector((state: RootState) => state.user.family);
    const auth = useAuth();
    const { isAuthenticated } = auth;

    const toggleSidebar = () => setIsCollapsed(!isCollapsed);

    const toggleMobileSidebar = () => {
        setIsOpen(!isOpen);
        setIsCollapsed(false);
    };

    if (!isAuthenticated) {
        return <></>;
    }

    return (
        <>
            {/* Mobile Backdrop */}
            {isOpen && (
                <div
                    className="fixed inset-0 bg-black opacity-50 z-40 md:hidden"
                    onClick={toggleMobileSidebar}
                ></div>
            )}

            {/* Sidebar */}
            <div
                className={`fixed z-50 top-19 h-[calc(100vh-64px)] bg-gradient-to-b from-purple-100 via-purple-50 to-fuchsia-100 
          shadow-md transition-all duration-300 flex flex-col justify-items-start
          ${isCollapsed ? "w-9" : "w-56"}
          ${isOpen ? "left-0" : "-left-full"} md:left-0`}
            >
                {/* Collapse Button */}
                <div
                    className="cursor-pointer p-2 hover:bg-purple-200 transition-all duration-300"
                    onClick={toggleSidebar}
                >
                    {isCollapsed ? (
                        <HiChevronRight size={20} color="purple" />
                    ) : (
                        <HiChevronLeft size={20} color="purple" />
                    )}
                </div>

                {/* Links */}
                <div className={`p-4 space-y-4 ${isCollapsed ? "hidden" : "block"}`}>
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
                        href="/shoppinglist"
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

            {/* Mobile Toggle Button */}
            <button
                onClick={toggleMobileSidebar}
                className="md:hidden fixed bottom-4 right-4 p-3 bg-purple-600 text-white rounded-full shadow-lg hover:bg-purple-700 z-50"
            >
                ☰
            </button>
        </>
    );
}

// Sidebar Link Component
function SidebarLink({ href, icon, label, isCollapsed }: any) {
    return (
        <Link
            href={href}
            className={`flex items-center p-2 rounded-md transition-all 
                    hover:bg-purple-200 text-purple-600 
                    ${isCollapsed ? "justify-center" : "space-x-3"}`}
        >
            {icon}
            {!isCollapsed && (
                <span className="md:text-md md:font-medium sm:text-sm sm:font-medium">{label}</span>
            )}
        </Link>
    );
}