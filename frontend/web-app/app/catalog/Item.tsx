import { MdOutlineAddShoppingCart } from "react-icons/md";
import { FaRegEdit } from "react-icons/fa";
import EditItemModal from "./EditItemModal";
import { useState } from "react";
import { RootState } from "../redux/store";
import { useDispatch, useSelector } from "react-redux";
import toast from "react-hot-toast";
import { useAuth } from "../hooks/useAuth";
import { addShoppingListItems } from "../lib/fetchShoppingLists";
import { updateShoppingListInStore } from "../redux/shoppingListSlice";

interface ItemProps {
    name: string;
    id: string;
    categorySKU: string;
    sku: string;
    setEditedItem: (item: { id: string; name: string; categorySKU: string, sku: string }) => void;
    setIsEditItemModalOpen?: (action: boolean) => void;
    showEditItemButton: boolean
}

export default function ItemComponent({ name, id, categorySKU, sku, setEditedItem, setIsEditItemModalOpen, showEditItemButton }: ItemProps) {
    const shoppingList = useSelector((state: RootState) => state.shoppinglists.currentShoppingList);
    const { acquireToken } = useAuth();
    const dispatch = useDispatch();

    const handleAddItemToShoppingList = async () => {
        if (!shoppingList?.id) {
            toast.error("No shopping list selected");
            return;
        }
        await acquireToken();
        const updatedShoppingList = await addShoppingListItems(shoppingList?.id, { skus: [sku] });

        if (updatedShoppingList) {
            dispatch(updateShoppingListInStore(updatedShoppingList));
            toast.success("Items added to shopping list");
        } else {
            toast.error("Failed to add items to shopping list");
        }
    }

    const handleEditItemClick = () => {
        setEditedItem({ id, name, categorySKU, sku });
        setIsEditItemModalOpen != undefined ? setIsEditItemModalOpen(true) : null;
    }

    return (<div className="flex items-center bg-purple-50 border border-purple-300 rounded-lg p-2 shadow-sm">
        {/* Item Name */}
        <span className="text-sm text-indigo-800 font-medium mr-2">{name}</span>

        {/* Add to Shopping List Button */}
        <button
            onClick={handleAddItemToShoppingList}
            className="p-1 rounded-full text-indigo-800 hover:bg-purple-100"
            aria-label="Add to Shopping List"
        >
            <MdOutlineAddShoppingCart size={16} />
        </button>

        {/* Edit Item Button */}
        {showEditItemButton &&
            <button
                onClick={handleEditItemClick}
                className="p-1 rounded-full text-indigo-800 hover:bg-purple-100 ml-2"
                aria-label="Edit Item"
            >
                <FaRegEdit size={16} />
            </button>
        }
    </div>
    );
}