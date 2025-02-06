import { useRef, useEffect } from "react";
import { keymap, highlightActiveLineGutter } from "@codemirror/view";
import { EditorState } from "@codemirror/state";
import { defaultKeymap } from "@codemirror/commands";
import { javascript } from "@codemirror/lang-javascript"; // Use Quipper syntax later
import { EditorView, basicSetup } from "codemirror"

export const Editor = () => {
    const editorRef = useRef(null);

    useEffect(() => {
        if (!editorRef.current) return;

        const state = EditorState.create({
            doc: "Hello World",
            extensions: [basicSetup,  javascript()],
        });

        const view = new EditorView({
            state,
            parent: editorRef.current,
        });
        view.dom.style.textAlign = "left";


        return () => view.destroy();
    }, []);

    return <div ref={editorRef} style={{ border: "1px solid #ccc", padding: "10px" }}></div>;
};

export default Editor;
