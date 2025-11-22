import { useEffect, useState } from 'react';
import './App.css';
import { Editor } from "./Editor"
import { MyButton } from "./MyButton"
import { OutputBox } from "./OutputBox"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./components/ui/Tabs"


function App() {


    const [output, setOutput] = useState("");
    const [imageUrl, setImageUrl] = useState("");
    const [code, setCode] = useState(""); 
    const [files, setFiles] = useState({})

    useEffect(() => {
        fetch("api/CodeExecution/getCodeExamples")
            .then((response) => response.json())
            .then((data) => setFiles(data))
            .catch((error) => console.error("Error fetching data:", error));
    }, []);

    const handleButtonClick = async () => {

        
        const requestBody = {
            language: "haskell",
            code: code,
        }

        const response = await fetch("api/CodeExecution/execute", {
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

    let defaultCode = `import Quipper

main :: IO ()
main = print_simple EPS (qinit False >>= hadamard)`

    return (
        <div>
            <h1>Qiupper code compiler</h1>
            <p>To use this tool correctly, make sure you are outputing the result to either EPS or PDF format. PS still outputs the circuit but on a smaller scale.</p>
            <p>You can check the examples provided in case of question</p>
            <Tabs defaultValue="code" className="w-[400px]">
                <TabsList>
                    <TabsTrigger value="code">Code</TabsTrigger>
                    {Object.keys(files).map((key) => (
                        <TabsTrigger key={key} value={key}>
                            {key}
                        </TabsTrigger>
                    ))}
                </TabsList>

                <TabsContent value="code">
                    <Editor code={defaultCode} setCode={setCode} />
                </TabsContent>

                {Object.entries(files).map(([key, content]) => (
                    <TabsContent key={key} value={key}>
                        <Editor code={content} setCode={setCode} />
                    </TabsContent>
                ))}
            </Tabs>
            <MyButton onClick={handleButtonClick} label="Submit" />
            <OutputBox output={output} imageUrl={imageUrl} />

        </div>
    );
    

}

export default App;