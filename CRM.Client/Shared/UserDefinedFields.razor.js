export function RadioOptionValue(name) {
    var output = "";

    var el = document.getElementsByName(name);

    if (el != undefined && el != null && el.length > 0) {
        for (var i = 0; i < el.length; i++) {
            if (el[i].checked) {
                output = el[i].value;
            }
        }
    }

    return output;
}