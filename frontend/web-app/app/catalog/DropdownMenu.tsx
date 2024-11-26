import { Dropdown } from 'flowbite-react'
import React from 'react'
import toast from 'react-hot-toast'

interface DropdownMenuProps {
    handleShowOnlyItems: () => void;
    showOnlyItems: boolean
}

export default function DropdownMenu({ handleShowOnlyItems, showOnlyItems }: DropdownMenuProps) {
    return (
        <Dropdown size="sm"
            placement="bottom-start"
            inline
        >
            <Dropdown.Item className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-t-lg" onClick={() => toast.success("Create new category feature coming soon!")}>
                Create New Category
            </Dropdown.Item>
            <Dropdown.Item className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700" onClick={() => toast.success("Create new item feature coming soon!")}>
                Create New Item
            </Dropdown.Item>
            <Dropdown.Item className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-b-md" onClick={handleShowOnlyItems}>
                {showOnlyItems ? "Show Categories" : "Hide Categories"}
            </Dropdown.Item>

        </Dropdown>
    )
}
