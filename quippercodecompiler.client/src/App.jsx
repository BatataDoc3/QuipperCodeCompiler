import { useEffect, useState } from 'react';
import './App.css';
import CodeMirror from "@uiw/react-codemirror";
import { Editor }  from "./Editor"

function App() {

    const [code, setCode] = useState("// Write your Quipper code here");


    return (
        <div>
            <h1> Hello World! </h1>
            <p>This component demonstrates fetching data from the server.</p>
            <Editor />

        </div>
    );
    

}

export default App;