export function SetAriaLabel(element, ariaLabel) {
    var el = document.querySelector("#" + element + " .ime-text-area");
    if (el != undefined && el != null) {
        el.ariaLabel = ariaLabel;
    }
    // var editor = getEditorByContainerId(element);
    // if (editor != undefined && editor != null) {
    //     editor.updateOptions({
    //         'bracketPairColorization.enabled': false 
    //     });
    // }
}

// function getEditorByContainerId(containerId) {
//     const targetContainer = document.getElementById(containerId);
//     if (!targetContainer) return null;

//     // monaco.editor.getEditors() returns an array of all active instances
//     const allEditors = monaco.editor.getEditors();

//     // Find the editor whose DOM node lives inside your target container
//     return allEditors.find(editor => {
//         const domNode = editor.getDomNode();
//         return domNode && targetContainer.contains(domNode);
//     }) || null;
// }