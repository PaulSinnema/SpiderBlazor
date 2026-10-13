// Pointer-based drag & tap for mouse, touch and pen
window.spider = (() => {
    let ref = null, board = null, observer = null, drag = null;

    function init(dotnet, el) {
        ref = dotnet;
        board = el;
        observer = new ResizeObserver(report);
        observer.observe(board);
        board.addEventListener('pointerdown', down);
        window.addEventListener('pointermove', move, { passive: false });
        window.addEventListener('pointerup', up);
        window.addEventListener('pointercancel', cancel);
        report();
    }

    function dispose() {
        observer?.disconnect();
        board?.removeEventListener('pointerdown', down);
        window.removeEventListener('pointermove', move);
        window.removeEventListener('pointerup', up);
        window.removeEventListener('pointercancel', cancel);
        ref = board = observer = drag = null;
    }

    function report() {
        const r = board.getBoundingClientRect();
        ref?.invokeMethodAsync('OnResize', r.width, r.height);
    }

    function down(e) {
        if (drag || e.button !== 0) return;
        const card = e.target.closest('.cards .card');
        if (!card) return;
        e.preventDefault();
        drag = {
            col: +card.dataset.col,
            idx: +card.dataset.idx,
            x: e.clientX,
            y: e.clientY,
            id: e.pointerId,
            movable: card.classList.contains('movable'),
            moved: false,
            els: []
        };
    }

    function move(e) {
        if (!drag || e.pointerId !== drag.id) return;
        const dx = e.clientX - drag.x, dy = e.clientY - drag.y;
        if (!drag.moved) {
            if (!drag.movable || Math.hypot(dx, dy) < 6) return;
            drag.moved = true;
            drag.els = [...board.querySelectorAll(`.cards .card[data-col="${drag.col}"]`)]
                .filter(el => +el.dataset.idx >= drag.idx);
            drag.els.forEach(el => el.classList.add('dragging'));
        }
        e.preventDefault();
        drag.els.forEach(el => el.style.transform = `translate(${dx}px, ${dy}px)`);
    }

    async function up(e) {
        if (!drag || e.pointerId !== drag.id) return;
        const d = drag;
        drag = null;

        if (!d.moved) {
            await ref?.invokeMethodAsync('OnTap', d.col, d.idx);
            return;
        }

        // Target = column nearest to the centre of the dragged card
        const r = d.els[0].getBoundingClientRect();
        const to = columnAt(r.left + r.width / 2);
        d.els.forEach(el => { el.classList.remove('dragging'); el.classList.add('lifted'); });

        if (to >= 0 && to !== d.col) await ref?.invokeMethodAsync('OnDrop', d.col, d.idx, to);

        // On success Blazor already rewrote the style (slides to target); otherwise slide back
        d.els.forEach(el => el.style.transform = '');
        setTimeout(() => d.els.forEach(el => el.classList.remove('lifted')), 300);
    }

    function cancel(e) {
        if (!drag || e.pointerId !== drag.id) return;
        drag.els.forEach(el => { el.classList.remove('dragging'); el.style.transform = ''; });
        drag = null;
    }

    function columnAt(x) {
        let best = -1, bestDist = Infinity;
        board.querySelectorAll('.slot').forEach((s, i) => {
            const r = s.getBoundingClientRect();
            const dist = Math.abs(x - (r.left + r.width / 2));
            if (dist < bestDist) { bestDist = dist; best = i; }
        });
        return best;
    }

    const language = () => navigator.language || "en";

    return { init, dispose, language };
})();
