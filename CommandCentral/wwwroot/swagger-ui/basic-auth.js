var basicAuthUI =
    '<div class="input"><input placeholder="username" id="input_username" name="username" type="text"></div>' +
    '<div class="input"><input placeholder="password" id="input_password" name="password" type="password"></div>' +
    '<div class="input"><input id="input_submit" name="input_submit" type="button" value="Authenticate" onclick="submitAuthentication();"></div>';
$(basicAuthUI).insertBefore('#select_document');
$("#select_document").hide();
$("#logo > img").hide();
$("#input_baseUrl").hide();
$(".logo__title").html("Command Central REST API");

function submitAuthentication() {
    var username = $('#input_username').val();
    var password = $('#input_password').val();

    $.ajax({
        type: "POST",
        url: "/api/authentication",
        dataType: "json",
        headers: { "X-Api-Key": "E28235AC-57A1-42AC-AA85-1547B755EA7E" },
        data: JSON.stringify({ username: username, password: password }),
        contentType: "application/json; charset=utf-8",
        success: function (data, status, xhr) {
            var sessionId = xhr.getResponseHeader("X-Session-Id");

            $("input[name='X-Session-Id']").each(function () {
                $(this).val(sessionId);
            })
        }
    });
}

