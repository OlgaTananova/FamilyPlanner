export default function getIdToken(): string | null {
    // Search for the MSAL ID token key in session storage
    let msalTokenKey: string | undefined;

    for (const key in sessionStorage) {
        if (key.includes("msal.token.keys")) {
            msalTokenKey = key;
            break; 
        }
    }

    if (!msalTokenKey) {
        console.warn("MSAL token key not found in session storage.");
        return null;
    }

    // Retrieve and parse the token key object
    const tokenKeyObject = sessionStorage.getItem(msalTokenKey);
    if (!tokenKeyObject) {
        console.warn("Token key object not found in session storage.");
        return null;
    }

    let idTokenKey: string | undefined;
    try {
        const parsedTokenKeyObject = JSON.parse(tokenKeyObject);
        idTokenKey = parsedTokenKeyObject.idToken?.[0];
    } catch (error) {
        console.error("Failed to parse token key object:", error);
        return null;
    }

    if (!idTokenKey) {
        console.warn("ID token key not found in token key object.");
        return null;
    }

    // Retrieve and parse the ID token object
    const idTokenObject = sessionStorage.getItem(idTokenKey);
    if (!idTokenObject) {
        console.warn("ID token object not found in session storage.");
        return null;
    }

    try {
        const parsedIdToken = JSON.parse(idTokenObject);
        const idToken = parsedIdToken.secret;
        if (idToken) {
            return idToken;
        }
    } catch (error) {
        console.error("Failed to parse ID token object:", error);
        return null;
    }

    console.warn("ID token not found in the parsed ID token object.");
    return null;
};