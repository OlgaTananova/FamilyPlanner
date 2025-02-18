// utils/getGraphToken.ts
import { ClientSecretCredential } from "@azure/identity";

const tenantId = process.env.AZURE_AD_B2C_TENANT_ID;
const clientId = process.env.AZURE_AD_B2C_CLIENT_ID;
const clientSecret = process.env.AZURE_AD_B2C_SECRET;
const clientExtensionId = process.env.AZURE_AD_B2C_CLIENT_EXTENSION_ID_WITHOUT_DASH;


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
    const token = await getGraphToken();
    const graphApiUrl = "https://graph.microsoft.com/beta/users"
    const response = await fetch(graphApiUrl, {
        method: 'GET',
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });


    const users = await extractUsersFromResponse(response);

    const filteredUsers = users
        .filter(
            (user: any) =>
                user[`extension_${clientExtensionId}_Family`] === familyName
        )
        .map((user: any) => ({
            id: user.id,
            givenName: user.givenName,
            family: user[`extension_${clientExtensionId}_Family`] || "",
            role: user[`extension_${clientExtensionId}_Role`],
            isAdmin: user[`extension_${clientExtensionId}_IsAdmin`],
            email: user.mail || user.otherMails?.[0] || user.identities[0].issuerAssignedId
        }));

    return filteredUsers;
}