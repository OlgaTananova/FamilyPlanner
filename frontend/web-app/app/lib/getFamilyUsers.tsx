// utils/getGraphToken.ts
import { ClientSecretCredential } from "@azure/identity";

const tenantId = process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_ID;
const clientId = process.env.NEXT_PUBLIC_AZURE_AD_B2C_CLIENT_ID;
const clientSecret = process.env.NEXT_PUBLIC_AZURE_AD_B2C_SECRET;
const clientExtensionId = process.env.NEXT_PUBLIC_AZURE_AD_B2C_CLIENT_EXTENSION_ID_WITHOUT_DASH;


const scope = `${process.env.GRAPH_API_SCOPE}.default`;

const credential = new ClientSecretCredential(tenantId!, clientId!, clientSecret!);

async function getGraphToken(): Promise<string> {
    const tokenResponse = await credential.getToken(scope);
    return tokenResponse.token;
}

async function extractUsersFromResponse(response: Response) {
    if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
    }

    // Convert ReadableStream to JSON
    const jsonData = await response.json();

    // Access the 'value' property which contains the list of users
    const users = jsonData.value;

    // Return the extracted users
    return users;
}

export async function getFamilyUsers(familyName: string) {
    try {
        const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/family/${familyName}/users`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
            next: { revalidate: 0 }, // disable cache if you want fresh data
        });

        if (!res.ok) {
            throw new Error(`Failed to fetch family users: ${res.status}`);
        }

        const users = await res.json();
        return users;
    } catch (error) {
        console.error("Error fetching family users:", error);
        return [];
    }
}