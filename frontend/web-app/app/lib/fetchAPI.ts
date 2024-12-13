import { getAccessToken } from "./getAccessToken";
import toast from "react-hot-toast";


export default async function fetchApi<T>(
    serviceUrl: string,
    endpoint: string,
    options: RequestInit
): Promise<T | null> {
    const accessToken = getAccessToken();

    if (!serviceUrl || !accessToken) {
        toast.error("API URL or Access Token is not available.");
        console.error("API URL or Access Token is not available.");
        return null;
    }

    try {
        const response = await fetch(`${serviceUrl}${endpoint}`, {
            ...options,
            headers: {
                ...options.headers,
                Authorization: `Bearer ${accessToken}`,
            },
        });

        if (!response.ok) {
            const contentType = response.headers.get("Content-Type");
            // Try to parse the response as JSON if it's JSON
            if (contentType && contentType.includes("application/json")) {
                const errorResponse = await response.json();
                const errorMessage = errorResponse?.message || "An error occurred.";
                throw new Error(errorMessage);
            }

            // Otherwise, treat it as plain text
            const errorText = await response.text();
            throw new Error(errorText || "An error occurred.");
        }

        // Handle 204 No Content
        if (response.status === 204) {
            return null; // No content to return
        }

        return (await response.json()) as T;
    } catch (error: any) {
        const errorMessage = error?.message || "An unexpected error occurred.";
        toast.error("There is an error while getting the data from the server.");
        console.error("Fetch API Error:", errorMessage);
        return null;
    }
}
