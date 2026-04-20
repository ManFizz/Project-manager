const MAX_FILE_SIZE = 15 * 1024 * 1024;
const ALLOWED_TYPES = [
    "image/png",
    "image/jpeg",
    "image/jpg",
    "image/webp",
    "application/pdf",
    "video/mp4",
    "video/webm",
    "video/quicktime"
];

let selectedFiles = [];

function handleFiles(files) {

    Array.from(files).forEach(file => {

        if (file.size > MAX_FILE_SIZE) {
            showError(`${file.name} is too large`);
            return;
        }

        if (!file.type || !ALLOWED_TYPES.includes(file.type)) {
            showError(`${file.name} unsupported format`);
            return;
        }

        selectedFiles.push(file);
    });

    syncInputFiles();
    renderFileList();
}

function syncInputFiles() {
    const dt = new DataTransfer();
    selectedFiles.forEach(f => dt.items.add(f));
    document.getElementById('file-input').files = dt.files;
}

function removeFile(index) {
    selectedFiles.splice(index, 1);
    syncInputFiles();
    renderFileList();
}