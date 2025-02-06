import { Button, Modal, Textarea } from "flowbite-react";
import React, { useState } from "react";
import toast from "react-hot-toast";
import { FaCopy, FaEnvelope, FaTelegram, FaWhatsapp } from "react-icons/fa";
import { ShoppingList } from "../redux/shoppingListSlice"; // Import your ShoppingList type

interface SendShoppingListProps {
    isOpen: boolean;
    onClose: () => void;
    shoppingList: ShoppingList;
}

export default function SendShoppingList({ isOpen, onClose, shoppingList }: SendShoppingListProps) {
    const [message, setMessage] = useState<string>("");

    // Generate Message
    const generateMessage = () => {
        let text = `🛒 ${shoppingList.heading}\n`;
        text += shoppingList.items.map((item, index) => (
            `${index + 1}. ${item.name} - ${item.quantity} ${item.unit}`
        )).join("\n");
        setMessage(text);
    };

    // Handle Copy to Clipboard
    const handleCopyToClipboard = () => {
        navigator.clipboard.writeText(message);
        toast.success("Shopping list copied to clipboard!");
    };

    // Send via WhatsApp
    const handleSendWhatsApp = () => {
        const encodedMessage = encodeURIComponent(message);
        window.open(`https://wa.me/?text=${encodedMessage}`, "_blank");
    };

    // Send via Telegram
    const handleSendTelegram = () => {
        const encodedMessage = encodeURIComponent(message);
        window.open(`https://t.me/share/url?url=${encodedMessage}`, "_blank");
    };

    // Send via Email
    const handleSendEmail = () => {
        const subject = encodeURIComponent(`Shopping List: ${shoppingList.heading}`);
        const body = encodeURIComponent(message);
        window.location.href = `mailto:?subject=${subject}&body=${body}`;
    };

    // Initialize message when the modal opens
    React.useEffect(() => {
        if (isOpen) {
            generateMessage();
        }
    }, [isOpen]);

    return (
        <Modal show={isOpen} onClose={onClose}>
            <Modal.Header>Send Shopping List</Modal.Header>
            <Modal.Body>
                <div className="space-y-4">
                    {/* Display the message */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Message</label>
                        <Textarea
                            value={message}
                            rows={8}
                            readOnly
                            className="w-full border-gray-300 rounded-lg focus:ring-purple-500 focus:border-purple-500"
                        />
                    </div>

                    {/* Buttons */}
                    <div className="flex flex-wrap gap-4">
                        <Button color="green" onClick={handleSendWhatsApp} className="flex items-center gap-2">
                            <FaWhatsapp className="mr-1 mt-1" />
                            WhatsApp
                        </Button>
                        <Button color="blue" onClick={handleSendTelegram} className="flex items-center gap-2">
                            <FaTelegram className="mr-1 mt-1" />
                            Telegram
                        </Button>
                        <Button color="gray" onClick={handleSendEmail} className="flex items-center gap-2">
                            <FaEnvelope className="mr-1 mt-1" />
                            Email
                        </Button>
                        <Button color="purple" onClick={handleCopyToClipboard} className="flex items-center gap-2">
                            <FaCopy className="mr-1 mt-1" />
                            Copy
                        </Button>
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button color="gray" onClick={onClose}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}
