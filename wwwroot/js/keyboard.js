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
    // Only handle vim navigation keys: h, j, k, l
    if (['h', 'j', 'k', 'l'].includes(event.key)) {
        event.preventDefault(); // Prevent default browser behavior

        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('HandleKeyPress', event.key);
        }
    }
}

// Cleanup function
window.unregisterKeyHandler = () => {
    document.removeEventListener('keydown', handleKeyDown);
    dotNetHelper = null;
};
