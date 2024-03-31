﻿import { getHeight } from "../../modules/utility.js?v=8.3.9"
import Data from "../../modules/data.js?v=8.3.9"

export function init(id) {
    const con = {
        element: document.getElementById(id)
    }
    con.body = con.element.querySelector('.card-body')
    con.window = con.element.querySelector('.console-window')

    Data.set(id, con)
}

export function update(id) {
    const con = Data.get(id)
    const scroll = con.element.getAttribute('data-bb-scroll') === 'auto'
    if (scroll) {
        con.body.scrollTo(0, getHeight(con.window))
    }
}

export function dispose(id) {
    Data.remove(id)
}
