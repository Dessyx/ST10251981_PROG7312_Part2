(function() {
    const btn = document.getElementById('copyBtn');
    const input = document.getElementById('refNum');

    if (!btn || !input) return;

    btn.addEventListener('click', async function() {
        try {
            await navigator.clipboard.writeText(input.value);
        } catch (e) {
            input.select();
            document.execCommand('copy');
        }

        btn.classList.add('copied');
        setTimeout(() => btn.classList.remove('copied'), 1200);
    });
})();

