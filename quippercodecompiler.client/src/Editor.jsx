import { useRef, useEffect } from "react";
import { EditorState } from "@codemirror/state";
import { EditorView} from "@codemirror/view";
import { oneDark } from "@codemirror/theme-one-dark"; 
import { basicSetup } from "codemirror";
import { StreamLanguage } from "@codemirror/language";
import { haskell } from "@codemirror/legacy-modes/mode/haskell";


export const Editor = ({code, setCode }) => {
    const editorRef = useRef(null);

    useEffect(() => {
        if (!editorRef.current) return;

        const state = EditorState.create({
            doc: code, // Default content
            extensions: [
                basicSetup,
                oneDark,
                StreamLanguage.define(haskell),
                EditorView.updateListener.of((update) => {
                    if (update.docChanged) {
                        const newCode = update.state.doc.toString();
                        setCode(newCode); 
                    }
                })
            ],
        });

        const view = new EditorView({
            state,
            parent: editorRef.current,
        });
        view.dom.style.textAlign = "left";
        setCode(code);
        return () => view.destroy();
    }, [setCode]); 

    return <div ref={editorRef} style={{ border: "1px solid #ccc", padding: "10px" }}></div>;
};

export default Editor;

