const list = document.getElementById("list");
const form = document.getElementById("form");
const input = document.getElementById("text");

console.log("hello world!");

async function load() {
    const res = await fetch("/api/messages");
    const items = await res.json();
    list.innerHTML = items
        .slice()
        .reverse()
        .map(
            (m) =>
                `<li><strong>${escapeHtml(
                    m.text
                )}</strong> <span class="dim">— ${new Date(
                    m.createdAt
                ).toLocaleString()}</span></li>`
        )
        .join("");
}

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const text = input.value.trim();
    if (!text) return;

    await fetch("/api/messages", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ text }),
    });

    input.value = "";
    await load();
});

function escapeHtml(s) {
    return s.replace(
        /[&<>"']/g,
        (c) =>
            ({
                "&": "&amp;",
                "<": "&lt;",
                ">": "&gt;",
                '"': "&quot;",
                "'": "&#39;",
            }[c])
    );
}

load();
