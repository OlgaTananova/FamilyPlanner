import { Button, Modal } from 'flowbite-react';
import React from 'react';

interface FilterShoppingListsModalProps {
    isFilterModalOpen: boolean;
    setFilterModalOpen: React.Dispatch<React.SetStateAction<boolean>>;
    filterStartDate: string;
    setFilterStartDate: React.Dispatch<React.SetStateAction<string>>;
    filterEndDate: string;
    setFilterEndDate: React.Dispatch<React.SetStateAction<string>>;
    applyDateFilter: () => void;
}

export default function FilterShoppingListsModal({ isFilterModalOpen,
    setFilterModalOpen,
    filterEndDate,
    filterStartDate,
    setFilterEndDate,
    setFilterStartDate,
    applyDateFilter }: FilterShoppingListsModalProps) {

    return (
        <Modal show={isFilterModalOpen} onClose={() => setFilterModalOpen(false)}>
            <Modal.Header>Filter by Date</Modal.Header>
            <Modal.Body>
                <div className="space-y-4">
                    <div>
                        <label className="block mb-2 text-sm font-medium text-gray-700">Start Date</label>
                        <input
                            type="date"
                            onChange={(e) => setFilterStartDate(e.target.value)}
                            className="w-full border-gray-300 rounded-lg focus:ring-purple-500 focus:border-purple-500"
                        />
                    </div>
                    <div>
                        <label className="block mb-2 text-sm font-medium text-gray-700">End Date</label>
                        <input
                            type="date"
                            onChange={(e) => setFilterEndDate(e.target.value)}
                            className="w-full border-gray-300 rounded-lg focus:ring-purple-500 focus:border-purple-500"
                        />
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button color="purple" onClick={applyDateFilter}>
                    Apply Filter
                </Button>
                <Button color="light" onClick={() => setFilterModalOpen(false)}>
                    Cancel
                </Button>
            </Modal.Footer>
        </Modal>
    )
}
