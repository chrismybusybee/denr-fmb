$(document).ready(function () {
    $('.dataTable').DataTable({
            "language": {
            "search": "",
            "searchPlaceholder": "Search"
        }
    });

    var tableRows = document.querySelectorAll("table tbody tr");
    var maxHeight = 0;

    tableRows.forEach(function (row) {
            var rowHeight = row.clientHeight;
            if (rowHeight > maxHeight) {
                maxHeight = rowHeight;
        }
    });

    tableRows.forEach(function (row) {
        row.style.height = maxHeight + "px";
    });
});