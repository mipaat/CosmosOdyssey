// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const elements = document.querySelectorAll(".date-time-local");
for (const element of elements) {
    element.classList.remove("date-time-local");
    const culture = element.getAttribute("culture");
    element.textContent = (new Date(element.textContent)).toLocaleString(culture);
}