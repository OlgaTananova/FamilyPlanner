import {
    Button,
    Modal,
    ModalBody,
    ModalFooter,
    ModalHeader, Textarea
} from "flowbite-react";
import { useMemo } from "react";
import toast from "react-hot-toast";
import { FaCopy, FaEnvelope, FaTelegram, FaWhatsapp } from "react-icons/fa";
import { ShoppingList } from "../redux/shoppingListSlice"; // Import your ShoppingList type

interface SendShoppingListProps {
    isOpen: boolean;
    onClose: () => void;
    shoppingList: ShoppingList;
}

export default function SendShoppingList({ isOpen, onClose, shoppingList }: SendShoppingListProps) {


    const message = useMemo(() => {
        let text = `🛒 ${shoppingList.heading}\n\n`;

        text += shoppingList.items
            .map((item, index) => {
                const escapedItemName = item.name.replace(
                    /[_*[\]()~`>#+\-=|{}.!]/g,
                    "\\$&"
                );

                return `${index + 1} ${escapedItemName} ${item.quantity} ${item.unit}`;
            })
            .join("\n");

        return text;
    }, [shoppingList]);

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
    const handleSendTelegram = async () => {
        const encodedMessage = encodeURIComponent(message);
        window.open(`https://t.me/share/url?url=${encodedMessage}`, "_blank");

    };

    // Send via Email
    const handleSendEmail = () => {
        const subject = encodeURIComponent(`Shopping List: ${shoppingList.heading}`);
        const body = encodeURIComponent(message);
        window.location.href = `mailto:?subject=${subject}&body=${body}`;
    };


    return (
        <Modal show={isOpen} onClose={onClose}>
            <ModalHeader>Send Shopping List</ModalHeader>
            <ModalBody>
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
            </ModalBody>
            <ModalFooter>
                <Button color="gray" onClick={onClose}>
                    Close
                </Button>
            </ModalFooter>
        </Modal>
    );
}
