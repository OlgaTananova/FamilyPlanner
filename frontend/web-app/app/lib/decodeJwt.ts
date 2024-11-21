import toast from "react-hot-toast";

export default function decodeJwt(token: string): any {
    try {
        const base64Url = token.split(".")[1];
        const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split("")
                .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
                .join("")
        );
        return JSON.parse(jsonPayload);
    } catch (error) {
        toast.error("Failed to decode JWT:");
        console.error("Failed to decode JWT:", error);
        return null;
    }
}