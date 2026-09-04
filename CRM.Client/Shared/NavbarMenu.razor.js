// NavbarMenu.razor.js - keyboard navigation for the NavbarMenu component.
//
// WHY THIS FILE EXISTS
//   NavbarMenu is written in C#; opening, expanding branches and searching are
//   all Blazor renders. The one thing C# cannot do is move keyboard focus
//   between rows, so that lives here. Nothing else does: no state, no styling,
//   no Bootstrap plugin calls. If this file went missing the menu would still
//   work with the mouse and with Tab; only the arrow keys, Home/End and Escape
//   would stop.
//
// HOW IT IS LOADED
//   Blazor publishes a file named <Component>.razor.js beside its component, so
//   NavbarMenu.razor imports "./Shared/NavbarMenu.razor.js" once, on its first
//   render, and calls Init with the menu's <li> element. One listener per menu.
//
// WHY THE LISTENER IS ON THE MENU ELEMENT
//   Bootstrap registers its own keydown handler on the document for anything
//   inside a .dropdown-menu. Our panel uses that class for its looks but is not
//   a Bootstrap dropdown, so that handler would misfire on Up/Down/Escape. A
//   keydown reaches this element before it bubbles to the document, and the
//   keys handled here are stopped (stopPropagation) so Bootstrap never sees them.
//
// KEYS
//   Down / Up      next / previous visible row, wrapping; Down on the closed toggle opens the menu
//   Home / End     first / last row in the panel
//   Right          expand a collapsed branch; on an open branch, move into it
//   Left           collapse an open branch; otherwise move up to the parent branch
//   Escape         close the menu and put focus back on the toggle
//   anything else  left alone (Tab, Enter, Space and typing all keep their native behaviour)
//
// A "row" is anything focusable in the open panel - the search box, branch
// buttons and links - plus the toggle itself. Rows inside a collapsed branch
// are not in the DOM at all, and rows hidden by the search are not rendered,
// so nothing hidden is ever focused.
export function Init(root) {
    if (root == undefined || root == null) {
        return;
    }

    root.addEventListener("keydown", e => {
        // The toggle is the first .dropdown-toggle in the li (branch buttons carry the
        // class too, but they sit inside the panel, after the toggle). "open" is the panel
        // while it has Bootstrap's .show class, i.e. while the C# side has _open = true.
        const toggle = root.querySelector(".dropdown-toggle"), open = root.querySelector(".dropdown-menu.show");

        // The focusable rows in visual order, with the toggle at index 0. offsetParent is
        // null for anything not laid out, which filters out rows in a hidden panel.
        const rows = [toggle, ...root.querySelectorAll(".dropdown-menu.show :is(input, button, a.dropdown-item)")].filter(el => el.offsetParent !== null);

        // Where the key was pressed: which row, whether it is the search box, whether it
        // is a branch button (only branches carry aria-expanded) and whether that branch is open.
        const t = e.target, i = rows.indexOf(t), input = t.tagName === "INPUT", branch = t.matches("button[aria-expanded]"), expanded = t.getAttribute("aria-expanded") === "true";
        const focus = el => el && el.focus();

        // One entry per key. Returning false means "not handled here" so the key keeps its
        // default action (eg: Home/End and Left inside the search box move the caret).
        // A branch is expanded or collapsed by clicking it, which runs the C# ToggleBranch;
        // Escape clicks the toggle, which runs the C# TogglePanel and closes the menu.
        const keys = {
            ArrowDown: () => open ? focus(rows[(i + 1) % rows.length]) : toggle.click(),
            ArrowUp: () => focus(rows[(i - 1 + rows.length) % rows.length]),
            Home: () => input ? false : focus(rows[1] || rows[0]),
            End: () => input ? false : focus(rows[rows.length - 1]),
            ArrowRight: () => branch ? (expanded ? focus(rows[i + 1]) : t.click()) : false,
            ArrowLeft: () => input ? false : branch && expanded ? t.click() : focus(root.querySelector(`[aria-controls="${t.closest("ul[id]")?.id}"]`)),
            Escape: () => open ? (focus(toggle), toggle.click()) : false
        };

        // Ignore keys from outside the menu's rows, keys with no entry above, and entries
        // that declined. Everything else is ours: stop the browser's default and stop the
        // event before it reaches Bootstrap's document-level handler.
        if (i < 0 || !keys[e.key] || keys[e.key]() === false) return;
        e.preventDefault(); e.stopPropagation();
    });
}