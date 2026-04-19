let currentStep = 1;
const totalSteps = 5;

function showStep(step) {
    document.querySelectorAll('.wizard-step').forEach(s => s.style.display = 'none');
    document.getElementById(`step-${step}`).style.display = 'block';

    const progress = (step / totalSteps) * 100;
    document.getElementById('progress-bar').style.width = `${progress}%`;
    document.getElementById('progress-bar').textContent = `Step ${step} of ${totalSteps}`;
}

async function searchEmployees(term, selectId, isMultiple = false) {
    try {
        const response = await fetch(`/Project/SearchEmployees?term=${encodeURIComponent(term || '')}`);
        const employees = await response.json();
        const select = document.getElementById(selectId);
        select.innerHTML = '';

        employees.forEach(emp => {
            const option = document.createElement('option');
            option.value = emp.id;
            option.textContent = `${emp.firstName} ${emp.lastName} (${emp.mail})`;
            select.appendChild(option);
        });
    } catch (error) {
        console.error('[ERROR] SearchEmployees failed:', error);
    }
}

// Drag & Drop
const dropZone = document.getElementById('drop-zone');
const fileInput = document.getElementById('file-input');
const fileList = document.getElementById('file-list');

if (dropZone) {
    dropZone.addEventListener('click', () => fileInput.click());
    dropZone.addEventListener('dragover', e => { e.preventDefault(); dropZone.style.background = '#e9ecef'; });
    dropZone.addEventListener('dragleave', () => dropZone.style.background = '#f8f9fa');
    dropZone.addEventListener('drop', e => {
        e.preventDefault();
        dropZone.style.background = '#f8f9fa';
        handleFiles(e.dataTransfer.files);
    });
    fileInput.addEventListener('change', () => handleFiles(fileInput.files));
}

function handleFiles(files) {
    Array.from(files).forEach(file => {
        const li = document.createElement('div');
        li.className = 'list-group-item';
        li.textContent = `${file.name} (${(file.size / 1024).toFixed(1)} KB)`;
        fileList.appendChild(li);
    });
}

document.addEventListener('DOMContentLoaded', () => {
    showStep(1);

    // Load all employees
    searchEmployees('', 'manager-select', false);
    searchEmployees('', 'employees-select', true);

    // Live search
    document.getElementById('manager-search')?.addEventListener('input', e => searchEmployees(e.target.value, 'manager-select'));
    document.getElementById('employees-search')?.addEventListener('input', e => searchEmployees(e.target.value, 'employees-select', true));

    // Next/Prev buttons
    document.querySelectorAll('.next-step').forEach(btn => {
        btn.addEventListener('click', () => { currentStep = parseInt(btn.dataset.next); showStep(currentStep); });
    });
    document.querySelectorAll('.prev-step').forEach(btn => {
        btn.addEventListener('click', () => { currentStep = parseInt(btn.dataset.prev); showStep(currentStep); });
    });
});