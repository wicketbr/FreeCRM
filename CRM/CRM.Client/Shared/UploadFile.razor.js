export function CopyPasswordToClipboard(password) {
    navigator.clipboard.writeText(password);
    console.log("Password Copied to Clipboard");
}