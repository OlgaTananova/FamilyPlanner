import { Button } from "flowbite-react";
import { FaArchive } from "react-icons/fa";
import { HiShoppingCart } from "react-icons/hi";

interface ShoppingListButtonProps {
  heading: string;
  itemCount: number;
  isArchived: boolean,
  onClick: () => void
}

export default function ShoppingListButton({ heading, itemCount, isArchived, onClick }: ShoppingListButtonProps) {
  return (
    <button
      color="light"
      onClick={onClick}
      className="w-full flex justify-between items-center px-4 py-3 bg-gray-50 hover:bg-purple-100 rounded-lg border border-gray-300 shadow-sm transition duration-200"
    >
      {/* Shopping Cart Icon */}
      <HiShoppingCart className="text-purple-600 w-6 h-6 flex-shrink-0" />

      {/* Heading */}
      <span className="flex-grow text-center text-gray-800 font-medium md:text-sm">{heading}</span>

      {/* Item Count */}
      {isArchived && <FaArchive className="text-blue-400 w-4 h-4 flex-shrink-0 mr-2"/>}
      <span className="bg-purple-500 text-white text-sm w-6 h-6 flex items-center justify-center rounded-full flex-shrink-0">
        {itemCount}
      </span>
    </button>
  );
}
