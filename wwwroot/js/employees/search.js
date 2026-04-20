async function searchEmployees(term, selectId) {
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

    $.validator.unobtrusive.parse("#wizard-form");
}

function initEmployeeSearch() {
    searchEmployees('', 'manager-select');
    searchEmployees('', 'employees-select');

    document.getElementById('manager-search')?.addEventListener('input',
        e => searchEmployees(e.target.value, 'manager-select'));

    document.getElementById('employees-search')?.addEventListener('input',
        e => searchEmployees(e.target.value, 'employees-select'));
}