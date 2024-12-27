import React, { useEffect, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../redux/store';
import { Button } from 'flowbite-react';
import { Item } from '../redux/catalogSlice';
import { addShoppingListItems } from '../lib/fetchShoppingLists';
import toast from 'react-hot-toast';
import { useAuth } from '../hooks/useAuth';
import { updateShoppingListInStore } from '../redux/shoppingListSlice';

export default function AddItemsFromList() {
    const catalogItems = useSelector((state: RootState) => state.categories.itemsWOCategories);
    const shoppingListId = useSelector((state: RootState) => state.shoppinglists.currentShoppingList?.id);
    const [availableItems, setAvailableItems] = useState<Item[]>([]);
    const [selectedItems, setSelectedItems] = useState<string[]>([]);
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const dispatch = useDispatch();
    const { acquireToken } = useAuth();


    useEffect(() => {
        const items = [...catalogItems]?.sort((a, b) => a.name.localeCompare(b.name));
        setAvailableItems(items);
    }, [catalogItems]);

    // Handle Dropdown Toggle
    const toggleDropdown = () => setIsDropdownOpen(!isDropdownOpen);

    const handleSelectItem = (item: Item) => {
        // Toggle selection
        if (selectedItems.some((i) => i === item.sku)) {
            setSelectedItems(selectedItems.filter((i) => i !== item.sku));
        } else {
            setSelectedItems([...selectedItems, item.sku]);
        }
    };

    //TODO: Add selected items to the shopping list
    const handleAddSelectedItems = async () => {
        if (!shoppingListId) {
            toast.error("No shopping list selected");
            return;
        }
        await acquireToken();
        const updatedShoppingList = await addShoppingListItems(shoppingListId, { skus: selectedItems });

        if (updatedShoppingList) {
            dispatch(updateShoppingListInStore(updatedShoppingList));
            toast.success("Items added to shopping list");
            setIsDropdownOpen(false);
            setSelectedItems([]);
        } else {
            toast.error("Failed to add items to shopping list");
        }
    };

    return (

        <div className="mb-6 relative" >
            <Button
                color="purple"
                className="w-full text-left"
                onClick={toggleDropdown}
            >
                Add Items
            </Button>
            {
                isDropdownOpen && (
                    <div className="absolute z-10 mt-2 w-full bg-white border border-gray-300 shadow-lg rounded-lg">
                        <ul className="max-h-60 overflow-y-auto p-2 space-y-2">
                            {availableItems?.map((item) => (
                                <li
                                    key={item.sku}
                                    className={`flex items-center p-2 hover:bg-purple-100 rounded-lg cursor-pointer`}
                                    onClick={() => handleSelectItem(item)}
                                >
                                    <input
                                        type="checkbox"
                                        checked={selectedItems.some((i) => i === item.sku)}
                                        onChange={() => handleSelectItem(item)}
                                        className="w-4 h-4 text-purple-500 focus:ring-purple-500 mr-2"
                                    />
                                    {item.name}
                                </li>
                            ))}
                        </ul>
                        <div className="p-4 flex justify-between border-t">
                            <Button color="purple" size="sm" onClick={() => handleAddSelectedItems()}>
                                Add Selected
                            </Button>
                            <Button color="light" size="sm" onClick={() => { setIsDropdownOpen(false); setSelectedItems([]) }}>
                                Close
                            </Button>
                        </div>
                    </div>
                )
            }
        </div >
    )
}
