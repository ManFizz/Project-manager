function initUploader() {

    const dropZone = document.getElementById('drop-zone');
    const fileInput = document.getElementById('file-input');

    if (!dropZone) return;

    dropZone.addEventListener('click', () => fileInput.click());

    dropZone.addEventListener('dragover', e => {
        e.preventDefault();
        dropZone.classList.add('bg-light', 'border-primary');
    });

    dropZone.addEventListener('dragleave', () => {
        dropZone.classList.remove('bg-light', 'border-primary');
    });

    dropZone.addEventListener('drop', e => {
        e.preventDefault();
        dropZone.classList.remove('bg-light', 'border-primary');
        handleFiles(e.dataTransfer.files);
    });

    fileInput.addEventListener('change', () => handleFiles(fileInput.files));
}

function renderFileList() {
    const fileList = document.getElementById('file-list');
    fileList.innerHTML = '';

    selectedFiles.forEach((file, i) => {

        let preview = '';

        if (file.type.startsWith('image/')) {
            const url = URL.createObjectURL(file); // TODO: fix memory leak. revokeObjectURL after delete
            preview = `<img src="${url}" style="height:40px;margin-right:10px;" />`;
        }

        const div = document.createElement('div');
        div.className = 'list-group-item d-flex justify-content-between';

        div.innerHTML = `
            <div>${preview}${file.name}</div>
            <button class="btn btn-sm btn-danger" onclick="removeFile(${i})">X</button>
        `;

        fileList.appendChild(div);
    });
}

function showError(message) {
    const errorBox = document.getElementById('upload-errors');

    const div = document.createElement('div');
    div.className = 'alert alert-danger py-1';
    div.textContent = message;

    errorBox.appendChild(div);

    setTimeout(() => div.remove(), 4000);
}