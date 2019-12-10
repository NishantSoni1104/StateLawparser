$(document).ready(function () {
    var lineItemBlobId = 0;
    var serialize = function (obj) {
        const str = [];
        for (let p in obj)
            if (obj.hasOwnProperty(p)) {
                str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
            }
        return str.join("&");
    };

    var dataFileId = $("#dataFileId").val();
    var dataTable = $('#tblItemNumberBlobs').DataTable({
        columns: [
            {
                data: "id",
                visible: false,
                searchable: false
            },
            { data: "itemNumber" },
            { data: "category" },
            { data: "description" },
            {
                "autoWidth": false,
                "render": function (data, type, row) {
                    return '<a id="btnEdit' + row.id + '" class="line-item-edit btn btn-info">Edit</a>';
                }
            },
            {
                autoWidth: false,
                data: null,
                render: function (data, type, row) {
                    return '<a id="btnDelete' + row.id + '" class="delete-class btn btn-info">Delete</a>';
                }
            }
        ],
        ajax: "/api/lineItemBlob/dataFileId/" + dataFileId,
        initComplete: function () {
            $(".delete-class")
                .on("click",
                    function (event) {
                        lineItemBlobId = event.target.id.substring(9);
                        $('#deleteLineItemBlob').modal('show');
                    });

            $(".line-item-edit")
                .on("click",
                    function (event) {
                        lineItemBlobId = event.target.id.substring(7);
                        $.get("/api/lineItemBlob/id/" + lineItemBlobId)
                            .done(function (data) {
                                $("#txtEditItemNumber").val(data.itemNumber);
                                $("#txtEditCategory").val(data.category);
                                $("#txtEditDescription").val(data.description);
                                $('#EditLineItemBlob').modal('show');
                            });                        
                    });
        }
    });

    $('#btnAddLineItemComments')
        .on('click', function () {
            var itemNumber = $("#txtItemNumber").val();
            var category = $("#txtCategory").val();
            var description = $("#txtDescription").val();

            var urlSearchParams = serialize({
                dataFileId: dataFileId,
                itemNumber: itemNumber,
                category: category,
                description: description
            });

            $.post("/api/lineItemBlob?" + urlSearchParams)
                .done(function () {
                    dataTable.ajax.reload();
                });
        });

    function getDescription(itemNumber, category) {
        $.get("/lineitem/itemNumber/" + itemNumber + "/category/" + category)
            .done(function (data) {
                $("#txtDescription").val(data);
            });
    }

    $("#txtItemNumber").bindWithDelay("keyup keypress paste blur",
        function (event) {
            var category = $("#txtCategory").val();
            var itemNumber = event.target.value;
            if (category && itemNumber) {
                getDescription(itemNumber, category);
            }
        }, 1000);

    $("#txtCategory").bindWithDelay("keyup keypress paste blur",
        function (event) {
            var itemNumber = $("#txtItemNumber").val();
            var category = event.target.value;
            if (category && itemNumber) {
                getDescription(itemNumber, category);
            }
        }, 1000);

    $("#btnEditLineItemBlob")
        .on("click", function () {
            var itemNumber = $("#txtEditItemNumber").val();
            var category = $("#txtEditCategory").val();
            var description = $("#txtEditDescription").val();

            var urlSearchParams = serialize({
                dataFileId: dataFileId,
                itemNumber: itemNumber,
                category: category,
                description: description,
                id: lineItemBlobId
            });

            $.ajax({
                type: "PUT",
                url: '/api/lineItemBlob?' + urlSearchParams,
                cache: false
            }).done(function () {
                dataTable.ajax.reload();
                $('#EditLineItemBlob').modal('hide');
            }).fail(function () {
                console.error("Sorry. Server unavailable. ");
            });
        });

    $("#btnDelete")
        .on("click", function () {
            $.ajax({
                type: "DELETE",
                url: '/api/lineItemBlob/id/' + lineItemBlobId,
                cache: false
            }).done(function () {
                dataTable.ajax.reload();
                $('#deleteLineItemBlob').modal('hide');
            }).fail(function () {
                console.error("Sorry. Server unavailable. ");
            });
        });
});