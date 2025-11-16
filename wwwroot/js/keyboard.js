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

// Cleanup function
window.unregisterKeyHandler = () => {
    document.removeEventListener('keydown', handleKeyDown);
    dotNetHelper = null;
};

// Register dialog close handler to auto-save form data
window.registerDialogCloseHandler = (dialogElement, dotNetHelper) => {
    dialogElement.addEventListener('close', (e) => {
        const form = document.getElementById('keybindingForm');
        const formData = new FormData(form);

        // Get values from form
        const left = formData.get('left') || '';
        const down = formData.get('down') || '';
        const up = formData.get('up') || '';
        const right = formData.get('right') || '';

        // Validate no duplicates among non-empty values
        const values = [left, down, up, right].filter(v => v !== '');
        const hasDuplicates = values.length !== new Set(values).size;

        if (hasDuplicates) {
            e.target.showModal();
            alert('Error: Duplicate keys detected. Please use unique keys.');
            return;
        }

        // Call C# method to save
        dotNetHelper.invokeMethodAsync('OnDialogClose', left, down, up, right);
    });
};
