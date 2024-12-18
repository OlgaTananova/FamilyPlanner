import { MdOutlineAddShoppingCart } from "react-icons/md";
import { FaRegEdit } from "react-icons/fa";
import EditItemModal from "./EditItemModal";
import { useState } from "react";

interface ItemProps {
    name: string;
    id: string;
    categorySKU: string;
    sku: string;
    setEditedItem: (item: { id: string; name: string; categorySKU: string, sku: string }) => void;
    setIsEditItemModalOpen: (action: boolean) => void;
}

export default function ItemComponent({ name, id, categorySKU, sku, setEditedItem, setIsEditItemModalOpen}: ItemProps) {

    //const [isEditItemModalOpen, setIsEditItemModalOpen] = useState(false);

    const handleEditItemClick = ()=> {
        setEditedItem({id, name, categorySKU, sku});
        setIsEditItemModalOpen(true);
    }

    return (<div className="flex items-center bg-purple-50 border border-purple-300 rounded-lg p-2 shadow-sm">
        {/* Item Name */}
        <span className="text-sm text-indigo-800 font-medium mr-2">{name}</span>

        {/* Add to Shopping List Button */}
        <button
            onClick={() => { }}
            className="p-1 rounded-full text-indigo-800 hover:bg-purple-100"
            aria-label="Add to Shopping List"
        >
            <MdOutlineAddShoppingCart size={16} />
        </button>

        {/* Edit Item Button */}
        <button
            onClick={handleEditItemClick}
            className="p-1 rounded-full text-indigo-800 hover:bg-purple-100 ml-2"
            aria-label="Edit Item"
        >
            <FaRegEdit size={16} />
        </button>
        {/* <EditItemModal isOpen={isEditItemModalOpen} onClose={() => setIsEditItemModalOpen(false)} item={{
            id: id,
            name: name,
            categoryId: categoryId
        }} /> */}
    </div>
    );
}