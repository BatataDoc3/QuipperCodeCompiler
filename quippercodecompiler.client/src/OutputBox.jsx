import React from "react";

export const OutputBox = ({ output, imageUrl }) => {
    return (
        <div
            style={{
                marginTop: "20px",
                padding: "10px",
                backgroundColor: "#000000",
                border: "1px solid #ddd",
                borderRadius: "5px",
                fontFamily: "monospace",
                whiteSpace: "pre-wrap",
                maxHeight: "500px",
                overflowY: "auto",
                color: "#ffffff",
            }}
        >
            {output && <pre>{output}</pre>}

            {imageUrl && (
                <div style={{ marginTop: "10px", textAlign: "center" }}>
                    <img
                        src={imageUrl}
                        alt="Generated Circuit"
                        style={{ maxWidth: "100%", borderRadius: "5px" }}
                    />
                </div>
            )}

            {!output && !imageUrl && "No output yet..."}
        </div>
    );
};

export default OutputBox;
