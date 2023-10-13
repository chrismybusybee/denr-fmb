$(document).ready(function () {
    var dataTable = $('.dataTable').DataTable({
        "language": {
            "search": "",
            "searchPlaceholder": "Search",
        },
        
    });

    var adjustRowHeights = function () {
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
    };

    adjustRowHeights();

    dataTable.on('draw.dt', function () {
        feather.replace();

        adjustRowHeights();
    });
});