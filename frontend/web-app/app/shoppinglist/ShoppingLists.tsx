'use client'
import { Button, Dropdown, DropdownItem } from 'flowbite-react';
import {
    Dispatch,
    SetStateAction,
    useEffect,
    useMemo,
    useState
} from 'react';
import { HiPlus } from 'react-icons/hi';
import { useDispatch, useSelector } from 'react-redux';
import { setCurrentShoppingList, ShoppingList } from '../redux/shoppingListSlice';
import { RootState } from '../redux/store';
import AddShoppingListModal from './AddShoppingListModal';
import FilterShoppingListsModal from './FilterShoppingListsModal';
import ShoppingListButton from './ShoppingListButton';

interface ShoppingListsProps {
    activeSection: "lists" | "current" | "frequent";
    onSelectActiveSection: Dispatch<SetStateAction<"lists" | "current" | "frequent">>;
}

export default function ShoppingLists({ activeSection, onSelectActiveSection }: ShoppingListsProps) {
    const dispatch = useDispatch();
    const shoppingLists = useSelector((state: RootState) => state.shoppinglists.lists);
    const [isAddShoppingListModalOpen, setAddShoppingListModalOpen] = useState(false);
    const [showArchived, setShowArchived] = useState(true); // State to toggle archived visibility
    const [isFilterModalOpen, setFilterModalOpen] = useState(false);
    const [filterStartDate, setFilterStartDate] = useState<string>("");
    const [filterEndDate, setFilterEndDate] = useState<string>("");
    const [isFiltered, setIsFiltered] = useState(false);
    const [showTooltip, setShowTooltip] = useState(true);

    const visibleShoppingLists = useMemo(() => {
        let lists = showArchived
            ? shoppingLists
            : shoppingLists.filter((list) => !list.isArchived);

        if (isFiltered && filterStartDate && filterEndDate) {
            const startDate = new Date(filterStartDate);
            const endDate = new Date(filterEndDate);

            lists = lists.filter((list) => {
                const listDate = new Date(list.createdAt);
                return listDate >= startDate && listDate <= endDate;
            });
        }

        return lists;
    }, [
        shoppingLists,
        showArchived,
        isFiltered,
        filterStartDate,
        filterEndDate,
    ]);

    useEffect(() => {
        dispatch(
            setCurrentShoppingList(
                visibleShoppingLists.length > 0
                    ? visibleShoppingLists[0]
                    : null
            )
        );
    }, [visibleShoppingLists, dispatch]);

    useEffect(() => {
        const timer = setTimeout(() => setShowTooltip(false), 5000);
        return () => clearTimeout(timer);

    }, [])

    const handleToggleArchived = () => {
        setShowArchived(!showArchived);
    };

    const handleSelectList = async (list: ShoppingList) => {
        dispatch(setCurrentShoppingList(list));
        onSelectActiveSection("current");
    };

    const applyDateFilter = () => {
        setIsFiltered(true);
        setFilterModalOpen(false);
    };

    // Clear date filter
    const clearFilter = () => {
        setFilterStartDate("");
        setFilterEndDate("");
        setIsFiltered(false);
        setShowArchived(true);
    };

    return (
        <>
            <div className={`p-4 bg-purple-50 border border-purple-300 rounded-lg shadow-md ${activeSection === "lists" ? "block" : "hidden"} md:block`}>
                <div className="flex justify-between items-center mb-4">
                    <div className='relative'>
                        {/* Circle button for creating a new shopping list with a tooltip */}
                        <Button
                            size="xs"
                            color="purple"
                            className="rounded-full p-2 bg-purple-500 text-white hover:bg-purple-600 focus:ring-2 focus:ring-purple-400"
                            onClick={() => setAddShoppingListModalOpen(true)}
                        >
                            <HiPlus className="w-3 h-3" />
                        </Button>
                        {/* Tooltip */}
                        {showTooltip && (
                            <div className="absolute top-10 left-3 w-40 transform -translate-x-1/2 px-3 py-1 text-xs text-white bg-purple-500 rounded-lg shadow-lg animate-fade-in-out">
                                Add a new shopping list
                            </div>
                        )}
                    </div>
                    {/* Dropdown for actions */}
                    <Dropdown label="" inline placement="bottom-end" className="relative">
                        <DropdownItem onClick={() => setAddShoppingListModalOpen(true)}>
                            Create New Shopping List
                        </DropdownItem>
                        <DropdownItem onClick={() => handleToggleArchived()}>
                            {showArchived ? "Hide Archived Shopping Lists" : "Show Archived Shopping Lists"}
                        </DropdownItem>
                        <DropdownItem onClick={() => setFilterModalOpen(true)}>
                            Filter Shopping Lists by Date
                        </DropdownItem>
                    </Dropdown>
                </div>
                {/* Clear Filter Button */}
                {isFiltered && (
                    <div className="mb-4">
                        <Button
                            size="xs"
                            color="light"
                            onClick={clearFilter}
                            className="text-purple-700 border-purple-300 hover:bg-purple-100 focus:ring-purple-500"
                        >
                            Clear Date Filter
                        </Button>
                    </div>
                )}
                {visibleShoppingLists.length > 0 ?

                    <ul className="space-y-2">
                        {visibleShoppingLists.map((list) => (
                            <li key={list.id}>
                                <ShoppingListButton
                                    key={list.id}
                                    heading={list.heading}
                                    itemCount={list.items.length}
                                    isArchived={list.isArchived}
                                    onClick={() => handleSelectList(list)}
                                />
                            </li>

                        ))}
                    </ul> : <div>
                        No shopping lists
                    </div>
                }

            </div>
            <AddShoppingListModal
                isOpen={isAddShoppingListModalOpen}
                onClose={() => setAddShoppingListModalOpen(false)}
            />
            <FilterShoppingListsModal
                isFilterModalOpen={isFilterModalOpen}
                setFilterModalOpen={setFilterModalOpen}
                filterStartDate={''}
                setFilterStartDate={setFilterStartDate}
                filterEndDate={''}
                setFilterEndDate={setFilterEndDate}
                applyDateFilter={applyDateFilter} />
        </>
    )
}
