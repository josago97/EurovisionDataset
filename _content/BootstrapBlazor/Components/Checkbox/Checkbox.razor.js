import { setIndeterminate } from "../../modules/utility.js"
import EventHandler from "../../modules/event-handler.js"

export function init(id, invoke, method) {
    const el = document.getElementById(id);
    if (el === null) {
        return;
    }

    el.onclick = async function (e) {
        e.preventDefault();

        //EventHandler.on(el, 'click', e => {
        const stopPropagation = el.getAttribute("data-bb-stop-propagation");
        if (stopPropagation === "true") {
            e.stopPropagation();
        }

        const state = el.getAttribute("data-bb-state");
        let val = null;
        if (state) {
            val = state == "1" ? 0 : 1;
            el.removeAttribute('data-bb-state');

            if (state === "1") {
                el.parentElement.classList.remove('is-checked');
            }
            else {
                el.parentElement.classList.add('is-checked');
            }
        }

        e.preventDefault();

        const result = await invoke.invokeMethodAsync(method, val);
        if (result.isComplete && result.result) {

        }
    }
    //});
}

export function dispose(id) {
    const el = document.getElementById(id);
    if (el === null) {
        return;
    }

    EventHandler.off(el, 'click');
}

export { setIndeterminate }
