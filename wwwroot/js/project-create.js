document.addEventListener('DOMContentLoaded', () => {

    initWizard();
    initEmployeeSearch();
    initUploader();

    document.getElementById("wizard-form").addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(this);
        const xhr = new XMLHttpRequest();

        xhr.open("POST", this.action);

        xhr.upload.addEventListener("progress", function (e) {
            if (e.lengthComputable) {
                const percent = (e.loaded / e.total) * 100;
                updateUploadProgress(percent);
            }
        });

        xhr.onload = function () {
            if (xhr.status === 200)
                window.location.href = "/Project";
            else
                showError("Upload failed");
        };

        xhr.send(formData);
    });
});

function updateUploadProgress(percent) {
    const bar = document.getElementById("upload-progress");
    bar.style.width = percent + "%";
    bar.textContent = Math.round(percent) + "%";
}