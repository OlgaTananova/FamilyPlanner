import { Dropdown, DropdownItem } from 'flowbite-react';

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
            <DropdownItem onClick={() => setEditShoppingListModalOpen(true)} className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-t-lg">
                Edit Shopping List
            </DropdownItem>
            <DropdownItem className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-t-lg"
                onClick={() => setIsSendShoppingListModalOpen(!isSendShoppingListModalOpen)}>
                Send Shopping List
            </DropdownItem>
            <DropdownItem onClick={() => setIsHiddenCategories(!isHiddenCategories)} className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-t-lg">
                {isHiddenCategories ? "Show Categories" : "Hide Categories"}
            </DropdownItem>
        </ Dropdown>
    )
}
