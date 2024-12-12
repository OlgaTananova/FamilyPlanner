'use client'
import { Button } from 'flowbite-react';
import React, { useEffect, useState } from 'react'
import ShoppingListButton from './ShoppingListButton';
import CurrentShoppingList from './CurrentShoppingList';
import MobileViewToggles from './MobileViewToggles';
import ShoppingLists from './ShoppingLists';
import { setCurrentShoppingList, setShoppingLists, ShoppingList } from '../redux/shoppingListSlice';
import { useDispatch, useSelector } from 'react-redux';
import { useAuth } from '../hooks/useAuth';
import { RootState } from '../redux/store';
import { fetchShoppingListData } from '../lib/fetchShoppingLists';

export default function ShoppingListPage() {
    const shoppingLists = useSelector((state: RootState) => state.shoppinglists.lists);
    const currentShoppingList = useSelector((state: RootState) => state.shoppinglists.currentShoppingList);
    const [activeSection, setActiveSection] = useState<"lists" | "current" | "frequent">("lists");
    const dispatch = useDispatch();
    const { acquireToken } = useAuth();

    // Fetch ShoppingLists
    useEffect(() => {
        async function fetchData() {

            // make sure there is a valid token in the storage
            await acquireToken();
            // Fetch categories
            const fetchedShoppingLists = await fetchShoppingListData();
            console.log(fetchedShoppingLists);

            if (fetchedShoppingLists) {
                dispatch(setShoppingLists(fetchedShoppingLists));
                dispatch(setCurrentShoppingList(fetchedShoppingLists[0]));
            }
        }
        fetchData();

    }, []);


    return (
        <div className="container mx-auto px-4 py-6">
            {/* Header */}
            <h1 className="text-2xl font-bold text-purple-700 mb-6">Shopping Lists</h1>

            {/* Mobile View Toggles (Placed Below Heading) */}
            <MobileViewToggles onSetActiveSection={setActiveSection} activeSection={activeSection} />
            {/* Responsive Sections */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {/* First Column: Lists */}
                <ShoppingLists activeSection={activeSection} onSelectActiveSection={setActiveSection} />

                {/* Second Column: Current Shopping List */}
                <div className={`p-4 bg-white border border-gray-300 rounded-lg shadow-md ${activeSection === "current" ? "block" : "hidden"} md:block`}>
                    <CurrentShoppingList />
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
