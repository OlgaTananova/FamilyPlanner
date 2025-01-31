import { getAccessToken } from "./getAccessToken";
import toast from "react-hot-toast";


export default async function fetchApi<T>(
    serviceUrl: string,
    endpoint: string,
    options: RequestInit
): Promise<T | null> {
    const accessToken = getAccessToken();

    if (!serviceUrl || !accessToken) {
        console.error("API URL or Access Token is not available.");
        toast.error("API URL or Access Token is not available. Please login again.");
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

            let errorResponse;
            try {
                errorResponse = await response.json();
            } catch (jsonError) {
                errorResponse = null;
            }

            if (errorResponse) {
                if (errorResponse?.title && errorResponse?.detail) {
                    // Handling ProblemDetails format
                    const errorTitle = errorResponse.title;
                    const errorDetail = errorResponse.detail;
                    const traceId = errorResponse.traceId ? `Trace ID: ${errorResponse.traceId}` : "";

                    const fullErrorMessage = `${errorTitle} - ${errorDetail}`.trim();

                    toast.error(fullErrorMessage);
                    console.error("Fetch API Error:", fullErrorMessage);
                    return null;
                } else if (errorResponse?.message) {
                    // Handling message format    
                    toast.error(errorResponse.message);
                    console.error("Fetch API Error:", errorResponse.message);
                    return null;
                }
            } else {
                // Handling other errors
                const errorText = await response.text();
                toast.error(errorText || "An error occurred.");
                console.error("Fetch API Error:", errorText);
                return null;
            }
        }

        // Handle 204 No Content
        if (response.status === 204) {
            return null; // No content to return
        }
        // Return the OK response as JSON
        return (await response.json()) as T;
    } catch (error: any) {
        const errorMessage = error?.message || "An unexpected error occurred.";
        toast.error("There is an error while getting the data from the server.");
        console.error("Fetch API Error:", errorMessage);
        return null;
    }
}
