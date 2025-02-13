import React, { useState } from "react";

export const OutputBox = ({ output }) => {
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
                maxHeight: "200px",
                overflowY: "auto",
            }}
        >
            {output || "No output yet..."}
        </div>
    );
};


export default OutputBox;
