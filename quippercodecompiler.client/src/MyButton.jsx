import React, { useState } from "react";

export const MyButton = ({ onClick, label = "Click Me" }) => {
    const [loading, setLoading] = useState(false);

    const handleClick = async () => {
        setLoading(true);
        await onClick();
        setLoading(false);
    };

    return (
        <button
            onClick={handleClick}
            style={{
                padding: "10px 20px",
                backgroundColor: loading ? "#aaa" : "#4CAF50",
                color: "#fff",
                border: "none",
                borderRadius: "5px",
                cursor: loading ? "not-allowed" : "pointer",
            }}
            disabled={loading}
        >
            {loading ? "Loading..." : label}
        </button>
    );
};

export default MyButton;
