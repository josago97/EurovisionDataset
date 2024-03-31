import "./browser.js?v=8.3.9"
import { execute } from "./ajax.js?v=8.3.9"

export async function ping(url, invoke, method) {
    const info = browser()
    let data = {
        browser: info.browser + ' ' + info.version,
        device: info.device,
        language: info.language,
        engine: info.engine,
        userAgent: navigator.userAgent,
        os: info.system + ' ' + info.systemVersion
    }

    const result = await execute({
        method: 'GET',
        url
    });
    if (result) {
        data.ip = result.Ip;
        data.id = result.Id;
    }
    await invoke.invokeMethodAsync(method, data)
}
