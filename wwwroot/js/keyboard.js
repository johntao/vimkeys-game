// Keyboard handler for vim-style navigation (hjkl)

let dotNetHelper = null;

window.registerKeyHandler = (helper) => {
    dotNetHelper = helper;

    // Remove existing listener to avoid duplicates
    document.removeEventListener('keydown', handleKeyDown);

    // Add keydown event listener
    document.addEventListener('keydown', handleKeyDown);
};

function handleKeyDown(event) {
    if (dotNetHelper) {
        // Pass all event information to C# for centralized handling
        dotNetHelper.invokeMethodAsync('HandleKeyDown', event.key, event.ctrlKey, event.shiftKey, event.altKey, event.metaKey);
    }
}

window.getPropertyAny = (el, propName) => el?.[propName];
window.callFunctionAny = (el, funcName) => el?.[funcName]();
