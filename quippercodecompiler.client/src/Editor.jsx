import { useRef, useEffect } from "react";
import { keymap, highlightActiveLineGutter } from "@codemirror/view";
import { EditorState } from "@codemirror/state";
import { defaultKeymap } from "@codemirror/commands";
import { javascript } from "@codemirror/lang-javascript"; // Use Quipper syntax later
import { EditorView, lineNumbers, highlightActiveLine, drawSelection} from "@codemirror/view";
import { history, historyKeymap } from "@codemirror/commands";
import { oneDark } from "@codemirror/theme-one-dark";  // Import the one-dark theme
import { basicSetup } from "codemirror";


export const Editor = ({ setCode }) => {
    const editorRef = useRef(null);

    useEffect(() => {
        if (!editorRef.current) return;

        const state = EditorState.create({
            doc: "Hello World", // Default content
            extensions: [
                basicSetup,
                oneDark,
                javascript(),
                EditorView.updateListener.of((update) => {
                    if (update.docChanged) {
                        // This will capture changes in the editor
                        const newCode = update.state.doc.toString();
                        setCode(newCode);  // Update the state with the new content
                    }
                })
            ],
        });

        const view = new EditorView({
            state,
            parent: editorRef.current,
        });
        view.dom.style.textAlign = "left";

        return () => view.destroy();
    }, [setCode]);  // Ensure `setCode` is included as a dependency

    return <div ref={editorRef} style={{ border: "1px solid #ccc", padding: "10px" }}></div>;
};

export default Editor;

