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
        console.log("Button clicked!");
        await new Promise((resolve) => setTimeout(resolve, 2000));
        console.log("Operation complete!");
    };

    return (
        <div>
            <h1> Hello World! </h1>
            <p>This component demonstrates fetching data from the server.</p>
            <Editor />
            <MyButton onClick={handleButtonClick} label="Submit" />
            <OutputBox output={"test"} />

        </div>
    );
    

}

export default App;