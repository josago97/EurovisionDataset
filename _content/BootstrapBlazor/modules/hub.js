﻿import { getFingerCode } from "./utility.js?v=8.3.9"

export async function init(options) {
    const { invoke, method, interval = 3000 } = options;
    const code = getFingerCode();

    setInterval(async () => {
        await invoke.invokeMethodAsync(method, code);
    }, interval);
}
