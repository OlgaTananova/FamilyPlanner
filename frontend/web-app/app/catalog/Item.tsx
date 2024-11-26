import { MdOutlineAddShoppingCart } from "react-icons/md";
import { FaRegEdit } from "react-icons/fa";

interface ItemProps {
    name: string;
    onAddToCart: () => void;
    onEdit: () => void;
}

export default function ItemComponent({ name, onAddToCart, onEdit }: ItemProps) {
    return (<div className="flex items-center bg-purple-50 border border-purple-300 rounded-lg p-2 shadow-sm">
        {/* Item Name */}
        <span className="text-sm text-indigo-800 font-medium mr-2">{name}</span>

        {/* Add to Shopping List Button */}
        <button
            onClick={onAddToCart}
            className="p-1 rounded-full text-indigo-800 hover:bg-purple-100"
            aria-label="Add to Shopping List"
        >
            <MdOutlineAddShoppingCart size={16} />
        </button>

        {/* Edit Item Button */}
        <button
            onClick={onEdit}
            className="p-1 rounded-full text-indigo-800 hover:bg-purple-100 ml-2"
            aria-label="Edit Item"
        >
            <FaRegEdit size={16} />
        </button>
    </div>
    );
}