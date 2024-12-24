window.clipboardCopy = async (codeElement) => {
    await navigator.clipboard.writeText(codeElement.textContent)
}