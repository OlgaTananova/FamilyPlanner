export function getAccessToken(): string | null {
    try {
        // Find the "msal.token.keys" entry in session storage
        const tokenKeysObject = Object.keys(sessionStorage).find((key) =>
            key.includes("msal.token.keys")
        );

        if (!tokenKeysObject) {
            console.warn("Access token key not found in session storage.");
            return null;
        }

        // Retrieve and parse the token keys
        const tokenKeys = JSON.parse(sessionStorage.getItem(tokenKeysObject) || "{}");
        const accessTokenKey = tokenKeys.accessToken?.[0];

        if (!accessTokenKey) {
            console.warn("Access token key not found in token keys.");


            return null;
        }

        // Retrieve and parse the actual access token object
        const accessTokenObject = sessionStorage.getItem(accessTokenKey);
        const tokenData = accessTokenObject ? JSON.parse(accessTokenObject) : null;

        if (!tokenData?.secret) {
            console.warn("Access token secret not found in the access token object.");
            return null;
        }

        return tokenData.secret;
    } catch (error) {
        console.error("Error retrieving access token:", error);
        return null;
    }
}