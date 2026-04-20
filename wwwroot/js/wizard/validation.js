function validateStep(button) {

    const form = $("#wizard-form");
    const currentStepDiv = button.closest('.wizard-step');

    let isValid = true;

    $(currentStepDiv).find("input, select").each(function () {
        if (!$(this).valid()) {
            isValid = false;
        }
    });

    if (!isValid) return false;

    // custom date validation
    if (currentStep === 1) {
        const startInput = document.querySelector('[name="Start"]');
        const endInput = document.querySelector('[name="End"]');

        const start = startInput.value;
        const end = endInput.value;

        if (start && end && new Date(end) < new Date(start)) {

            const errorSpan = document.querySelector('[data-valmsg-for="End"]');
            errorSpan.textContent = "End date must be after start date";
            endInput.classList.add("input-validation-error");

            return false;
        } else {
            const errorSpan = document.querySelector('[data-valmsg-for="End"]');
            if (errorSpan) {
                errorSpan.textContent = "";
                endInput.classList.remove("input-validation-error");
            }
        }
    }

    return true;
}