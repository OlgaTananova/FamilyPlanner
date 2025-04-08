import { Dropdown } from 'flowbite-react';

interface DropdownMenuProps {
    handleShowOnlyItems?: () => void;
    showOnlyItems?: boolean;
    setIsAddCategoryModalOpen: (action: boolean) => void;
    setIsAddItemModalOpen: (action: boolean) => void;
}

export default function DropdownMenu({ handleShowOnlyItems, showOnlyItems, setIsAddCategoryModalOpen, setIsAddItemModalOpen }: DropdownMenuProps) {

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
        </>
    )
}
