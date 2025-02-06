// utils/getGraphToken.ts
import { ClientSecretCredential } from "@azure/identity";

// Replace these with your Azure AD B2C app details
const tenantId = process.env.NEXT_PUBLIC_AZURE_AD_B2C_TENANT_ID; // e.g., {your-tenant-name}.onmicrosoft.com
const clientId = process.env.NEXT_PUBLIC_AZURE_AD_B2C_CLIENT_ID;
const clientSecret = process.env.NEXT_PRIVATE_AZURE_AD_B2C_SECRET;

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
                user.extension_f6549fea4fbd4796b153c099dfa72e66_Family === familyName
        )
        .map((user: any) => ({
            id: user.id,
            givenName: user.givenName,
            family: user?.extension_f6549fea4fbd4796b153c099dfa72e66_Family || "",
            role: user.extension_f6549fea4fbd4796b153c099dfa72e66_Role,
            isAdmin: user.extension_f6549fea4fbd4796b153c099dfa72e66_IsAdmin,
            email: user.mail || user.otherMails?.[0] || user.identities[0].issuerAssignedId 
        }));

    return filteredUsers;
}