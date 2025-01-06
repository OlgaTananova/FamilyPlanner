'use client'
import React, { Dispatch, SetStateAction, useEffect, useState } from 'react';
import ShoppingListButton from './ShoppingListButton';
import { setCurrentShoppingList, setShoppingLists, ShoppingList } from '../redux/shoppingListSlice';
import { useDispatch, useSelector } from 'react-redux';
import { useAuth } from '../hooks/useAuth';
import { fetchShoppingListData } from '../lib/fetchShoppingLists';
import { RootState } from '../redux/store';
import { Button, Dropdown } from 'flowbite-react';
import { HiPlus } from 'react-icons/hi';
import AddShoppingListModal from './AddShoppingListModal';
import FilterShoppingListsModal from './FilterShoppingListsModal';

interface ShoppingListsProps {
    activeSection: "lists" | "current" | "frequent";
    onSelectActiveSection: Dispatch<SetStateAction<"lists" | "current" | "frequent">>;
}

export default function ShoppingLists({ activeSection, onSelectActiveSection }: ShoppingListsProps) {
    const dispatch = useDispatch();
    const shoppingLists = useSelector((state: RootState) => state.shoppinglists.lists);
    const [isAddShoppingListModalOpen, setAddShoppingListModalOpen] = useState(false);
    const [showArchived, setShowArchived] = useState(true); // State to toggle archived visibility
    const [visibleShoppingLists, setVisibleShoppingLists] = useState<ShoppingList[]>([]);
    const [isFilterModalOpen, setFilterModalOpen] = useState(false);
    const [filterStartDate, setFilterStartDate] = useState<string>("");
    const [filterEndDate, setFilterEndDate] = useState<string>("");
    const [isFiltered, setIsFiltered] = useState(false);
    const [showTooltip, setShowTooltip] = useState(true);

    useEffect(() => {
        const visibleShoppingLists = showArchived
            ? shoppingLists // Show all lists
            : shoppingLists.filter((list) => !list.isArchived);
        setVisibleShoppingLists(visibleShoppingLists);
        visibleShoppingLists.length > 0 ? dispatch(setCurrentShoppingList(visibleShoppingLists[0])) :
            dispatch(setCurrentShoppingList(null));
    }, [shoppingLists, showArchived]);

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
        if (visibleShoppingLists.length > 0) {
            const filteredLists = visibleShoppingLists.filter((list) => {
                const listDate = new Date(list.createdAt);
                const startDate = new Date(filterStartDate);
                const endDate = new Date(filterEndDate);
                return listDate >= startDate && listDate <= endDate;
            });
            setVisibleShoppingLists(filteredLists);
            setIsFiltered(true);
            setFilterModalOpen(false);
        }
    };

    // Clear date filter
    const clearFilter = () => {
        setVisibleShoppingLists(shoppingLists);
        setFilterStartDate("");
        setFilterEndDate("");
        setIsFiltered(false);
        setShowArchived(true);
        dispatch(setCurrentShoppingList(shoppingLists[0]));
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
                        <Dropdown.Item onClick={() => setAddShoppingListModalOpen(true)}>
                            Create New Shopping List
                        </Dropdown.Item>
                        <Dropdown.Item onClick={() => handleToggleArchived()}>
                            {showArchived ? "Hide Archived Shopping Lists" : "Show Archived Shopping Lists"}
                        </Dropdown.Item>
                        <Dropdown.Item onClick={() => setFilterModalOpen(true)}>
                            Filter Shopping Lists by Date
                        </Dropdown.Item>
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
