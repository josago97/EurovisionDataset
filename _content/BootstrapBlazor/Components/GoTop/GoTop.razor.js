﻿import Data from "../../modules/data.js?v=8.3.9"
import EventHandler from "../../modules/event-handler.js?v=8.3.9"

const getScrollElement = el => {
    let ele = el
    while (ele && ele.scrollHeight <= ele.clientHeight) {
        ele = ele.parentNode
    }
    return ele || window
}

export function init(id, target) {
    const el = document.getElementById(id)
    if (el === null) {
        return
    }
    const go = { el, target }
    Data.set(id, go)

    go.tip = new bootstrap.Tooltip(el)
    EventHandler.on(el, 'click', e => {
        e.preventDefault();

        const element = (target && document.querySelector(target)) || getScrollElement(el)
        element.scrollTop = 0
        go.tip.hide()
    })
}

export function dispose(id) {
    const go = Data.get(id)
    Data.remove(id)

    if (go) {
        EventHandler.off(go.el, 'click')
        go.tip.dispose()
        delete go.tip
    }
}
