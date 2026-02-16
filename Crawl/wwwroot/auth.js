window.login = async (email, password) => {
    const response = await fetch("/api/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ email, password })
    });

    return response.ok;
};
window.logout = async () => {
    const response = await fetch("/api/logout", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        }
    });
    return response.ok;
};