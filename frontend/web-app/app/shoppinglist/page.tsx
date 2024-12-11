'use client'
import { Button } from 'flowbite-react';
import React, { useState } from 'react'
import ShoppingListButton from './ShoppingListButton';
import CurrentShoppingList from './CurrentShoppingList';
import { mockShoppingLists } from '@/mockingData';
import MobileViewToggles from './MobileViewToggles';

export default function ShoppingListPage() {
    const [activeSection, setActiveSection] = useState<"lists" | "current" | "frequent">("lists");
    const [selectedList, setSelectedList] = useState(mockShoppingLists[0]);

    return (
        <div className="container mx-auto px-4 py-6">
            {/* Header */}
            <h1 className="text-2xl font-bold text-purple-700 mb-6">Shopping Lists</h1>

            {/* Mobile View Toggles (Placed Below Heading) */}
            <MobileViewToggles onSetActiveSection={setActiveSection} activeSection={activeSection}/>
            {/* Responsive Sections */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {/* First Column: Lists */}
                <div className={`p-4 bg-purple-50 border border-purple-300 rounded-lg shadow-md ${activeSection === "lists" ? "block" : "hidden"} md:block`}>
                    <ul className="space-y-2">
                        {mockShoppingLists.map((list) => (
                            <li>
                                <ShoppingListButton
                                    key={list.id}
                                    heading={list.heading}
                                    itemCount={list.items.length}
                                    isArchived={list.isArchived}
                                    onClick={() => setSelectedList(list)}
                                />
                            </li>

                        ))}
                    </ul>
                </div>

                {/* Second Column: Current Shopping List */}
                <div className={`p-4 bg-white border border-gray-300 rounded-lg shadow-md ${activeSection === "current" ? "block" : "hidden"} md:block`}>
                    {selectedList ? (
                        <CurrentShoppingList shoppingList={selectedList} />
                    ) : (
                        <p className="text-gray-600">Select a shopping list to view details.</p>
                    )}
                </div>

                {/* Third Column: Frequently Bought Items */}
                <div className={`p-4 bg-gray-50 border border-gray-300 rounded-lg shadow-md ${activeSection === "frequent" ? "block" : "hidden"} md:block`}>
                    <h2 className="text-lg font-semibold text-gray-800 mb-4">Frequently Bought Items</h2>
                    <ul className="space-y-2">
                        <li className="flex justify-between items-center">
                            <span>Apples</span>
                            <Button size="xs" color="purple">Add</Button>
                        </li>
                        <li className="flex justify-between items-center">
                            <span>Eggs</span>
                            <Button size="xs" color="purple">Add</Button>
                        </li>
                        {/* Add more items */}
                    </ul>
                </div>
            </div>
        </div>
    );
}
