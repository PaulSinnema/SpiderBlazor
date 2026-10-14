// Pointer-based drag & tap for mouse, touch and pen
window.spider = (() => {
    let ref = null, board = null, observer = null, drag = null, empty = null;

    // Chrome/Edge/Android offer an install prompt; keep it for the settings button
    let installEvt = null;
    addEventListener('beforeinstallprompt', e => { e.preventDefault(); installEvt = e; });
    addEventListener('appinstalled', () => { installEvt = null; });

    function installState() {
        if (matchMedia('(display-mode: standalone)').matches || navigator.standalone) return 'installed';
        if (installEvt) return 'prompt';
        const ios = /iphone|ipad|ipod/i.test(navigator.userAgent) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
        return ios ? 'ios' : 'manual';
    }

    async function install() {
        if (!installEvt) return installState();
        installEvt.prompt();
        const choice = await installEvt.userChoice;
        installEvt = null;
        return choice.outcome === 'accepted' ? 'installed' : 'dismissed';
    }

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

    // Build id of the running app (from SW/HTTP cache) and of the server (cache bypassed)
    async function localBuild() {
        try { return (await (await fetch('version.json')).json()).build; } catch { return null; }
    }

    async function serverBuild() {
        try { return (await (await fetch('version.json?t=' + Date.now(), { cache: 'no-store' })).json()).build; } catch { return null; }
    }

    // Returns the server build when it differs from the running one
    async function checkUpdate() {
        if (!navigator.onLine) return null;
        const [local, server] = await Promise.all([localBuild(), serverBuild()]);
        return server && local && server !== local ? server : null;
    }

    // Bypass GitHub Pages (max-age=600) + Safari HTTP cache, drop SW caches, reload fresh
    async function update() {
        if (!navigator.onLine) return 'offline';
        const server = await serverBuild();
        if (!server) return 'offline';
        const local = await localBuild();
        if (local === server) return 'current';

        try {
            const regs = await navigator.serviceWorker?.getRegistrations() ?? [];
            await Promise.all(regs.map(r => r.unregister()));
            const keys = await caches.keys();
            await Promise.all(keys.map(k => caches.delete(k)));

            // Re-fetch every asset with cache:'reload' so the HTTP cache holds the new files
            const core = ['index.html', 'version.json', 'service-worker.js', 'manifest.webmanifest', 'css/app.css', 'js/spider.js',
                '_framework/blazor.webassembly.js', '_framework/dotnet.js'];
            const list = await (await fetch('service-worker-assets.js', { cache: 'reload' })).text();
            const urls = [...list.matchAll(/"url":\s*"([^"]+)"/g)].map(m => m[1]);
            await Promise.all([...new Set([...core, ...urls])].map(u => fetch(u, { cache: 'reload' }).catch(() => { })));
        } catch { /* reload anyway */ }

        // Query string forces a fresh navigation instead of a cached index.html
        location.replace(location.pathname + '?v=' + encodeURIComponent(server));
        return 'ok';
    }

    // Completed run: cards fly one by one (Ace first) to the pile slot in the top row
    function flyToFoundation(ids, index) {
        const slot = board.querySelectorAll('.slot')[Math.max(0, Math.min(index, 9))];
        if (!slot) return Promise.resolve();
        const s = slot.getBoundingClientRect();
        const top = board.getBoundingClientRect().top;
        const els = ids
            .map(id => board.querySelector(`.cards .card[data-id="${id}"]`))
            .filter(Boolean)
            .sort((a, b) => b.getBoundingClientRect().top - a.getBoundingClientRect().top);
        els.forEach((el, i) => {
            const r = el.getBoundingClientRect();
            const delay = i * 45;
            el.classList.remove('dragging', 'lifted');
            el.style.zIndex = 3000 - i;
            el.style.transition = `transform .45s cubic-bezier(.3,.7,.3,1) ${delay}ms`;
            // Keep any drag offset as the start, then travel to the pile
            el.style.transform = `translate(${s.left - r.left + (parseFloat(getComputedStyle(el).transform.split(',')[4]) || 0)}px, ${top - r.top + (parseFloat(getComputedStyle(el).transform.split(',')[5]) || 0)}px) rotate(${(i % 2 ? 1 : -1) * 4}deg)`;
        });
        return new Promise(res => setTimeout(res, 480 + els.length * 45));
    }

    // Win celebration: ticker-tape / confetti shower on a full-screen canvas
    function celebrate(seconds = 6) {
        const canvas = document.createElement('canvas');
        canvas.className = 'confetti';
        document.body.appendChild(canvas);
        const ctx = canvas.getContext('2d');
        const dpr = window.devicePixelRatio || 1;
        const resize = () => {
            canvas.width = innerWidth * dpr;
            canvas.height = innerHeight * dpr;
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        };
        resize();
        addEventListener('resize', resize);

        const colors = ['#ffd60a', '#ff4d6d', '#4cc9f0', '#80ed99', '#f72585', '#ffffff', '#ff9f1c', '#9b5de5'];
        const pieces = [];
        const spawn = n => {
            for (let i = 0; i < n; i++) {
                const ribbon = Math.random() < .35;
                pieces.push({
                    x: Math.random() * innerWidth,
                    y: -20 - Math.random() * 60,
                    w: ribbon ? 4 + Math.random() * 3 : 7 + Math.random() * 6,
                    h: ribbon ? 18 + Math.random() * 16 : 4 + Math.random() * 6,
                    vx: (Math.random() - .5) * 2.5,
                    vy: 2 + Math.random() * 3.5,
                    rot: Math.random() * Math.PI * 2,
                    vr: (Math.random() - .5) * .25,
                    flip: Math.random() * Math.PI * 2,
                    vf: .08 + Math.random() * .12,
                    sway: Math.random() * Math.PI * 2,
                    color: colors[Math.floor(Math.random() * colors.length)]
                });
            }
        };

        // Opening bursts from both lower corners
        const burst = (x, dir) => {
            for (let i = 0; i < 70; i++) {
                const a = -Math.PI / 2 + dir * (.2 + Math.random() * .6);
                const s = 9 + Math.random() * 9;
                pieces.push({
                    x, y: innerHeight, w: 7 + Math.random() * 6, h: 4 + Math.random() * 6,
                    vx: Math.cos(a) * s, vy: Math.sin(a) * s,
                    rot: Math.random() * 6, vr: (Math.random() - .5) * .3,
                    flip: Math.random() * 6, vf: .1 + Math.random() * .1, sway: Math.random() * 6,
                    color: colors[Math.floor(Math.random() * colors.length)]
                });
            }
        };
        burst(0, 1);
        burst(innerWidth, -1);

        const end = performance.now() + seconds * 1000;
        let last = performance.now();
        const frame = now => {
            const dt = Math.min((now - last) / 16.7, 3);
            last = now;
            if (now < end - 2000) spawn(4);
            ctx.clearRect(0, 0, innerWidth, innerHeight);
            for (let i = pieces.length - 1; i >= 0; i--) {
                const p = pieces[i];
                p.vy = Math.min(p.vy + .12 * dt, 4.5);
                p.vx *= .99;
                p.sway += .05 * dt;
                p.x += (p.vx + Math.sin(p.sway) * .8) * dt;
                p.y += p.vy * dt;
                p.rot += p.vr * dt;
                p.flip += p.vf * dt;
                if (p.y > innerHeight + 40) { pieces.splice(i, 1); continue; }
                ctx.save();
                ctx.translate(p.x, p.y);
                ctx.rotate(p.rot);
                ctx.scale(1, Math.cos(p.flip));   // tumbling paper
                ctx.fillStyle = p.color;
                ctx.fillRect(-p.w / 2, -p.h / 2, p.w, p.h);
                ctx.restore();
            }
            if (now < end || pieces.length) requestAnimationFrame(frame);
            else { removeEventListener('resize', resize); canvas.remove(); }
        };
        requestAnimationFrame(frame);
    }

    return { init, dispose, language, flyToStock, flyToFoundation, update, checkUpdate, localBuild, celebrate, installState, install };
})();
