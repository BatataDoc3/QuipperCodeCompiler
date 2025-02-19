import { useEffect, useState } from 'react';
import './App.css';
import CodeMirror from "@uiw/react-codemirror";
import { Editor } from "./Editor"
import { MyButton } from "./MyButton"
import { OutputBox } from "./OutputBox"

function App() {


    const [output, setOutput] = useState("");  // Output from the execution
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

        const text = await response.text();  // Read response as text

        if (!text) {
            throw new Error("Empty response from server");
        }

        const data = JSON.parse(text); // Convert to JSON
        console.log(data);
        setOutput(data.output);
        console.log("Button clicked!");
        await new Promise((resolve) => setTimeout(resolve, 2000));
        console.log("Operation complete!");
    };

    return (
        <div>
            <h1> Hello World! </h1>
            <p>This component demonstrates fetching data from the server.</p>
            <Editor setCode={setCode} />
            <MyButton onClick={handleButtonClick} label="Submit" />
            <OutputBox output={output} />

        </div>
    );
    

}

export default App;