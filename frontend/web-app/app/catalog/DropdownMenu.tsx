import { Dispatch } from '@reduxjs/toolkit';
import { Dropdown } from 'flowbite-react'
import React, { useState } from 'react'
import toast from 'react-hot-toast'
import AddCategoryModal from './AddCategoryModal';
import AddNewItemModal from './AddItemModal';

interface DropdownMenuProps {
    handleShowOnlyItems: () => void;
    showOnlyItems: boolean;
}

export default function DropdownMenu({ handleShowOnlyItems, showOnlyItems }: DropdownMenuProps) {
    const [isAddCategoryModalOpen, setIsAddCategoryModalOpen] = useState(false);
    const [isAddItemModalOpen, setIsAddItemModalOpen] = useState(false);

    return (
        <>
            <Dropdown size="sm"
                placement="bottom-start"
                inline
            >
                <Dropdown.Item className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-t-lg" onClick={() => setIsAddCategoryModalOpen(true)}>
                    Create New Category
                </Dropdown.Item>
                <Dropdown.Item className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700" onClick={() => setIsAddItemModalOpen(true)}>
                    Create New Item
                </Dropdown.Item>
                <Dropdown.Item className="px-4 py-2 hover:bg-gray-100 text-sm text-gray-700 rounded-b-md" onClick={handleShowOnlyItems}>
                    {showOnlyItems ? "Show Categories" : "Hide Categories"}
                </Dropdown.Item>

            </Dropdown>
            <AddCategoryModal isOpen={isAddCategoryModalOpen} onClose={() => setIsAddCategoryModalOpen(false)} />
            <AddNewItemModal isOpen={isAddItemModalOpen} onClose={() => setIsAddItemModalOpen(false)} />
        </>
    )
}
