$("#select_document").hide();
$("#logo > img").hide();
$("#input_baseUrl").hide();
$(".logo__title").html("Command Central REST API");

jQuery("div").on("DOMNodeInserted", function () {
   $("select").prop("disabled", false);
});



