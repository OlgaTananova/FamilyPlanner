'use client'
import Link from 'next/link';
import { useState } from 'react';
import CurrentShoppingList from './CurrentShoppingList';
import FrequentItems from './FrequentItems';
import MobileViewToggles from './MobileViewToggles';
import ShoppingLists from './ShoppingLists';

export default function ShoppingListPage() {
    const [activeSection, setActiveSection] = useState<"lists" | "current" | "frequent">("lists");

    return (
        <div className="container mx-auto px-4 py-6 relative">
            {/* Header */}
            <h1 className="text-2xl font-bold text-purple-700 mb-6">Shopping Lists</h1>
            <Link href={("/")}
                className="absolute top-0 right-0 text-gray-600 hover:text-gray-900"
                aria-label="Close Shopping Lists"
            >
                ✖
            </Link>
            {/* Mobile View Toggles (Placed Below Heading) */}
            <MobileViewToggles onSetActiveSection={setActiveSection} activeSection={activeSection} />
            {/* Responsive Sections */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {/* First Column: Lists */}
                <ShoppingLists activeSection={activeSection} onSelectActiveSection={setActiveSection} />

                {/* Second Column: Current Shopping List */}
                <div className={`p-4 bg-purple-50 border border-purple-300 rounded-lg shadow-md ${activeSection === "current" ? "block" : "hidden"} md:block`}>
                    <CurrentShoppingList />
                </div>

                {/* Third Column: Frequently Bought Items */}
                <div className={`p-4 bg-gray-50 border border-gray-300 rounded-lg shadow-md ${activeSection === "frequent" ? "block" : "hidden"} md:block`}>
                    <FrequentItems />
                </div>
            </div>
        </div>
    );
}
