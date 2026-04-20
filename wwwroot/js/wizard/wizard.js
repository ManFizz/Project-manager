let currentStep = 1;
const totalSteps = 5;

function showStep(step) {
    document.querySelectorAll('.wizard-step').forEach(s => s.style.display = 'none');
    document.getElementById(`step-${step}`).style.display = 'block';

    const progress = (step / totalSteps) * 100;
    const bar = document.getElementById('progress-bar');

    bar.style.width = `${progress}%`;
    bar.textContent = `Step ${step} of ${totalSteps}`;
}

function initWizard() {
    showStep(1);

    document.querySelectorAll('.next-step').forEach(btn => {
        btn.addEventListener('click', function () {
            if (!validateStep(this)) return;

            currentStep = parseInt(this.dataset.next);
            showStep(currentStep);
        });
    });

    document.querySelectorAll('.prev-step').forEach(btn => {
        btn.addEventListener('click', () => {
            currentStep = parseInt(btn.dataset.prev);
            showStep(currentStep);
        });
    });
}