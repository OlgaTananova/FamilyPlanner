import React from "react";
import { Modal, Button } from "flowbite-react";

interface ConfirmationModalProps {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: () => void;
    title?: string;
    message?: string;
}

export default function ConfirmationModal({
    isOpen,
    onClose,
    onConfirm,
    title = "Confirm Deletion",
    message,
}: ConfirmationModalProps) {
    return (
        <Modal show={isOpen} onClose={onClose} size="md" className="rounded-lg">
            <Modal.Header>
                <p className="text-xl font-semibold text-gray-800">{title}</p>
            </Modal.Header>
            <Modal.Body>
                <p className="text-sm text-gray-600">{message}</p>
            </Modal.Body>
            <Modal.Footer>
                <div className="flex justify-end space-x-4">
                    <Button color="light" onClick={onClose}>
                        Cancel
                    </Button>
                    <Button color="red" onClick={onConfirm}>
                        Delete
                    </Button>
                </div>
            </Modal.Footer>
        </Modal>
    );
}
