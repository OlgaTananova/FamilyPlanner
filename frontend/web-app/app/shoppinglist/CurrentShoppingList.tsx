import React, { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { RootState } from "../redux/store";
import { ShoppingListItem } from "../redux/shoppingListSlice";
import { Dropdown } from "flowbite-react";
import EditShoppingListModal from "./EditShoppingListModal";
import { FaArchive } from "react-icons/fa";
import ShoppingListDropdownMenu from "./ShoppingListDropdownMenu";
import ShoppingListItems from "./ShoppingListItems";


export default function CurrentShoppingList() {
    const shoppingList = useSelector((state: RootState) => state.shoppinglists.currentShoppingList);
    const [groupedItems, setGroupedItems] = useState<Record<string, ShoppingListItem[]>>({});
    const [isEditShoppingListModalOpen, setEditShoppingListModalOpen] = useState(false);
    const [isHiddenCategories, setIsHiddenCategories] = useState(false);

    useEffect(() => {
        if (shoppingList?.items?.length) {
            const grouped = shoppingList.items.reduce<Record<string, ShoppingListItem[]>>((acc, item) => {
                acc[item.categoryName] = acc[item.categoryName] || [];
                acc[item.categoryName].push(item);
                return acc;
            }, {});
            setGroupedItems(grouped);
        } else {
            setGroupedItems({});
        }
    }, [shoppingList]);

    if (!shoppingList) {
        return <p className="text-gray-500">No shopping list selected.</p>;
    }


    return (
        <div>
            {/* Header and Menu*/}
            <div className="flex justify-between items-center mb-6">
                <div className="flex items-center">
                    <h2 className="text-lg font-semibold text-gray-800">{shoppingList.heading}</h2>
                    {shoppingList.isArchived && (
                        <FaArchive className="text-blue-400 w-4 h-4 flex-shrink-0 ml-3 mt-1" />
                    )}
                </div>
                {/* Dropdown Menu */}
                <ShoppingListDropdownMenu setEditShoppingListModalOpen={setEditShoppingListModalOpen}
                    isHiddenCategories={isHiddenCategories}
                    setIsHiddenCategories={setIsHiddenCategories} />
            </div>

            {/* Shopping List Items */}
            <ShoppingListItems isHiddenCategories={isHiddenCategories} groupedItems={groupedItems} shoppingList={shoppingList} />

            {/* Summary Section */}
            <div className="mt-6 p-4 bg-gray-50 border rounded-lg border-gray-300">
                <div className="flex justify-between items-center mb-2">
                    <span className="text-sm text-gray-600">Total Items</span>
                    <span className="text-sm font-semibold text-gray-800">
                        {Object.values(groupedItems).flat().reduce((sum, item) => sum + item.quantity, 0)}
                    </span>
                </div>
                <div className="flex justify-between items-center mb-2">
                    <span className="text-sm text-gray-600">Sales Tax</span>
                    <span className="text-sm font-semibold text-gray-800">
                        ${shoppingList.salesTax.toFixed(2)}
                    </span>
                </div>
                <div className="flex justify-between items-center">
                    <span className="text-sm text-gray-600">Total Amount</span>
                    <span className="text-sm font-semibold text-gray-800">
                        $
                        {(Object.values(groupedItems)
                            .flat()
                            .reduce((sum, item) => sum + item.price, 0)
                            + shoppingList.salesTax).toFixed(2)}
                    </span>
                </div>
            </div>
            <EditShoppingListModal
                isOpen={isEditShoppingListModalOpen}
                onClose={() => { setEditShoppingListModalOpen(false) }}
                shoppingList={shoppingList}
            />
        </div>
    );

}
