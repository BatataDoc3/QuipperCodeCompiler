import { useEffect, useState } from 'react';
import './App.css';
import CodeMirror from "@uiw/react-codemirror";
import { Editor } from "./Editor"
import { MyButton } from "./MyButton"
import { OutputBox } from "./OutputBox"

function App() {


    const [output, setOutput] = useState("");  // Output from the execution
    const [imageUrl, setImageUrl] = useState("");
    const [code, setCode] = useState("");  // Code entered in the editor

    const handleButtonClick = async () => {

        // Simulate an async operation
        const requestBody = {
            language: "javascript",
            code: code,
        }

        const response = await fetch("https://localhost:7024/api/CodeExecution/execute", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(requestBody)
        });

        if (response.headers.get("Content-Type").includes("image")) {
            const blob = await response.blob();
            setImageUrl(URL.createObjectURL(blob));
            setOutput("");
        } else {
            const text = await response.text();
            setOutput(text);
            setImageUrl("");
        }

    };

    return (
        <div>
            <h1> Hello World! </h1>
            <p>This component demonstrates fetching data from the server.</p>
            <Editor setCode={setCode} />
            <MyButton onClick={handleButtonClick} label="Submit" />
            <OutputBox output={output} imageUrl={imageUrl} />

        </div>
    );
    

}

export default App;