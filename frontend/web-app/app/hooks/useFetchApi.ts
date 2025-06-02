import { useCallback } from "react";
import toast from "react-hot-toast";
import { useAuth } from "../hooks/useAuth";

export function useFetchApi() {

    const { acquireToken, signIn } = useAuth();

    const fetchApi = useCallback(async <T>(
        serviceUrl: string,
        endpoint: string,
        options: RequestInit,
        maxRetries: number = 3
    ): Promise<T | null> => {

        const { accessToken } = await acquireToken(); // Get the valid token

        if (!serviceUrl) {
            toast.error("Network request failed. Try to reload the page.")
            return null;
        }

        if (!accessToken) {
            await acquireToken();
        }

        const backoffDelay = (retryCount: number) => Math.pow(2, retryCount) * 1000;

        for (let attempt = 0; attempt <= maxRetries; attempt++) {
            try {
                const response = await fetch(`${serviceUrl}${endpoint}`, {
                    ...options,
                    headers: {
                        ...options.headers,
                        Authorization: `Bearer ${accessToken}`,
                    },
                });

                if (response.ok) {
                    return response.status === 204 ? null : (await response.json()) as T;
                } else {
                    if ([500, 502, 503, 504].includes(response.status) && attempt < maxRetries) {
                        const delay = backoffDelay(attempt);
                        console.warn(`Retrying API call in ${delay}ms...`);
                        await new Promise(res => setTimeout(res, delay));
                        continue;
                    }
                    const clonedResponse = response.clone(); // Clone response to allow multiple reads

                    let errorResponse;
                    try {
                        errorResponse = await clonedResponse.json(); // Try to parse as JSON
                    } catch {
                        errorResponse = null;
                    }

                    const errorMessage =
                        errorResponse?.title && errorResponse?.detail
                            ? `${errorResponse.title} - ${errorResponse.detail}`
                            : errorResponse?.message || "An error occurred.";

                    console.error(`Fetch API Error (Attempt ${attempt + 1}/${maxRetries}):`, errorMessage);
                    return null; // If not a retryable error, exit
                }

            } catch (error: any) {
                console.error(`Fetch API Attempt ${attempt + 1}/${maxRetries} Failed:`, error.message);
                if (attempt < maxRetries) {
                    const delay = backoffDelay(attempt);
                    console.warn(`Retrying API call in ${delay}ms...`);
                    await new Promise(res => setTimeout(res, delay));
                } else {
                    toast.error("Network error: Please check your internet connection.");
                    return null;
                }
            }
        }

        return null; // Return null if all retries failed
    }, [acquireToken]);
    return { fetchApi }
}