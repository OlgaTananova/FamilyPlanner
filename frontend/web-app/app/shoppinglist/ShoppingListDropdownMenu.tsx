import { Dropdown } from 'flowbite-react'
import React from 'react'

interface ShoppingListDropdownMenuProps {
    setEditShoppingListModalOpen: (value: boolean) => void;
    isHiddenCategories: boolean;
    setIsHiddenCategories: (value: boolean) => void;
    isSendShoppingListModalOpen: boolean;
    setIsSendShoppingListModalOpen: (value: boolean) => void;
}

export default function ShoppingListDropdownMenu({
    setEditShoppingListModalOpen,
    setIsHiddenCategories,
    isHiddenCategories,
    isSendShoppingListModalOpen,
    setIsSendShoppingListModalOpen }: ShoppingListDropdownMenuProps) {
    return (
        < Dropdown
            size="sm"
            placement="bottom-start"
            inline
        >
            <Dropdown.Item onClick={() => setEditShoppingListModalOpen(true)} className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-t-lg">
                Edit Shopping List
            </Dropdown.Item>
            <Dropdown.Item className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-t-lg"
                onClick={() => setIsSendShoppingListModalOpen(!isSendShoppingListModalOpen)}>
                Send Shopping List
            </Dropdown.Item>
            <Dropdown.Item onClick={() => setIsHiddenCategories(!isHiddenCategories)} className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-t-lg">
                {isHiddenCategories ? "Show Categories" : "Hide Categories"}
            </Dropdown.Item>
        </ Dropdown>
    )
}
