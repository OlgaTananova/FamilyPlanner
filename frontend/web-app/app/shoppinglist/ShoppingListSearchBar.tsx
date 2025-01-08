import React, { useEffect, useRef, useState } from 'react'
import { Item } from '../redux/catalogSlice';
import { addShoppingListItems, searchShoppingListItems } from '../lib/fetchShoppingLists';
import { DiVim } from 'react-icons/di';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../redux/store';
import toast from 'react-hot-toast';
import { updateShoppingListInStore } from '../redux/shoppingListSlice';
import { useAuth } from '../hooks/useAuth';

export default function ShoppingListSearchBar() {
    const [searchTerm, setSearchTerm] = useState("");
    const [searchResults, setSearchResults] = useState<Item[]>([]);
    const [isSearching, setIsSearching] = useState(false);
    const [showResults, setShowResults] = useState(false);
    const shoppingList = useSelector((state: RootState) => state.shoppinglists.currentShoppingList);
    const dispatch = useDispatch();
    const { acquireToken } = useAuth();
    const resultsRef = useRef<HTMLDivElement | null>(null);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (resultsRef.current && !resultsRef.current.contains(event.target as Node)) {
                setShowResults(false); // Close the dropdown if clicked outside
            }
        };

        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    const fetchSearchResults = async (query: string) => {
        if (!query.trim()) {
            setSearchResults([]);
            return;
        }

        setIsSearching(true);

        try {
            await acquireToken();
            const results = await searchShoppingListItems(query);
            if (results) {
                setSearchResults(results);
            }
        } catch (error) {
            console.error("Failed to fetch search results:", error);
        } finally {
            setIsSearching(false);
        }
    };

    const handleAddItem = async (item: Item) => {
        try {
            if (!shoppingList) {
                toast.error("No shopping list selected.");
                return;
            }
            await acquireToken();
            const updatedShoppingList = await addShoppingListItems(shoppingList!.id, { skus: [item.sku] });

            if (updatedShoppingList) {
                // Update the shopping list in the store
                dispatch(updateShoppingListInStore(updatedShoppingList));
                toast.success(`${item.name} added to the shopping list!`);
            }

        } catch (error) {
            console.error(error);
            toast.error("Failed to add item.");
        }
    };


    useEffect(() => {
        const delayDebounce = setTimeout(() => {
            fetchSearchResults(searchTerm);
        }, 1000); // Debounce the API call by 300ms

        return () => clearTimeout(delayDebounce); // Clear timeout on component unmount
    }, [searchTerm]);


    return (

        < div ref={resultsRef} className="relative mb-4" >
            <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onFocus={() => setShowResults(true)}
                placeholder="Search items to add..."
                className="w-full px-4 py-2 border rounded-lg focus:ring-purple-500 focus:border-purple-500"
            />

            {searchTerm && (
                <button
                    type="button"
                    onClick={() => { setSearchTerm(""); setSearchResults([]) }} // Clear input when clicked
                    className="absolute inset-y-0 right-2 flex items-center text-gray-400 hover:text-gray-600"
                >
                    ✖
                </button>
            )
            }
            {isSearching && <p className="text-sm text-gray-500 mt-2">Searching...</p>}
            {
                showResults && searchResults?.length > 0 && (
                    <ul className="absolute z-10 w-full bg-white border border-gray-300 shadow-lg rounded-lg mt-1">
                        {searchResults.map((result) => (
                            <li
                                key={result.sku}
                                className="flex justify-between items-center p-2 hover:bg-purple-100 cursor-pointer"
                            >
                                <span>{result.name}</span>
                                <button
                                    onClick={() => handleAddItem(result)} // Add item to the shopping list
                                    className="text-sm text-purple-600 hover:text-purple-800 font-medium"
                                >
                                    Add
                                </button>
                            </li>
                        ))}
                        {/* Close Button */}
                        <div className="text-right p-2 border-t">
                            <button
                                onClick={() => setShowResults(false)}
                                className="text-sm text-gray-500 hover:text-purple-600"
                            >
                                Close
                            </button>
                        </div>
                    </ul>
                )
            }
        </ div >

    )
}
