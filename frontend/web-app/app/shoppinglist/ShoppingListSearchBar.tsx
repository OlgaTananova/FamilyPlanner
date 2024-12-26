import React, { useEffect, useState } from 'react'
import { Item } from '../redux/catalogSlice';
import { addShoppingListItem, searchShoppingListItems } from '../lib/fetchShoppingLists';
import { DiVim } from 'react-icons/di';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../redux/store';
import toast from 'react-hot-toast';
import { updateShoppingListInStore } from '../redux/shoppingListSlice';

export default function ShoppingListSearchBar() {
    const [searchTerm, setSearchTerm] = useState(""); // Holds the search query
    const [searchResults, setSearchResults] = useState<Item[]>([]); // Stores fetched results
    const [isSearching, setIsSearching] = useState(false);
    const shoppingList = useSelector((state: RootState) => state.shoppinglists.currentShoppingList);
    const dispatch = useDispatch();

    const fetchSearchResults = async (query: string) => {
        if (!query.trim()) {
            setSearchResults([]); // Clear results if query is empty
            return;
        }

        setIsSearching(true);

        try {
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

    // TODO: fix the comoponent
    const handleAddItem = async (item: Item) => {
        try {
            // Send request to add item to shopping list
            const updatedShoppingList = await addShoppingListItem(shoppingList!.id, { sku: item.sku });

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

        < div className="relative mb-4" >
            <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search items to add..."
                className="w-full px-4 py-2 border rounded-lg focus:ring-purple-500 focus:border-purple-500"
            />
            {isSearching && <p className="text-sm text-gray-500 mt-2">Searching...</p>}
            {
                searchResults.length > 0 && (
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
                    </ul>
                )
            }
        </ div >

    )
}
