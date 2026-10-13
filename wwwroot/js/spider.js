// Pointer-based drag & tap for mouse, touch and pen
window.spider = (() => {
    let ref = null, board = null, observer = null, drag = null, empty = null;

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
        if (!card) {
            // Tap on empty table (not stock/top row) toggles the menu
            if (!e.target.closest('.toprow')) empty = { x: e.clientX, y: e.clientY, id: e.pointerId };
            return;
        }
        e.preventDefault();
        drag = {
            col: +card.dataset.col,
            idx: +card.dataset.idx,
            x: e.clientX,
            y: e.clientY,
            id: e.pointerId,
            movable: card.classList.contains('movable'),
            moved: false,
            els: [],
            card,
            peeked: false,
            timer: null
        };
        // Press and hold on a fixed face-up card: show it fully until release
        if (!drag.movable && card.classList.contains('up')) {
            const d = drag;
            d.timer = setTimeout(() => { d.peeked = true; card.classList.add('peek'); }, 250);
        }
    }

    function endPeek(d) {
        clearTimeout(d.timer);
        d.card.classList.remove('peek');
    }

    function move(e) {
        if (!drag || e.pointerId !== drag.id) return;
        const dx = e.clientX - drag.x, dy = e.clientY - drag.y;
        if (!drag.moved) {
            if (!drag.peeked && Math.hypot(dx, dy) >= 6) clearTimeout(drag.timer);
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
        if (empty && e.pointerId === empty.id) {
            const still = Math.hypot(e.clientX - empty.x, e.clientY - empty.y) < 10;
            empty = null;
            if (still) await ref?.invokeMethodAsync('OnEmptyTap');
            return;
        }
        if (!drag || e.pointerId !== drag.id) return;
        const d = drag;
        drag = null;
        endPeek(d);
        if (d.peeked) return;

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
        empty = null;
        if (!drag || e.pointerId !== drag.id) return;
        endPeek(drag);
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

    // Undo of a deal: slide the given cards back onto the stock
    function flyToStock(ids) {
        const target = board.querySelector('.stock .card:last-child') || board.querySelector('.stock-slot');
        if (!target) return Promise.resolve();
        const t = target.getBoundingClientRect();
        ids.forEach((id, i) => {
            const el = board.querySelector(`.cards .card[data-id="${id}"]`);
            if (!el) return;
            const r = el.getBoundingClientRect();
            el.style.zIndex = 2000 + i;
            el.style.transition = `transform .32s ease-in ${i * 15}ms, opacity .32s ease-in ${i * 15}ms`;
            el.style.transform = `translate(${t.left - r.left}px, ${t.top - r.top}px) scale(.96)`;
            el.style.opacity = '.6';
        });
        return new Promise(res => setTimeout(res, 340 + ids.length * 15));
    }

    return { init, dispose, language, flyToStock };
})();
