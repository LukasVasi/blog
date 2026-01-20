// DifferentFrom attribute validation
$.validator.addMethod("differentfrom", function (value, element, params) {
    const getOtherElement = (element, otherProperty) => {
        var name = element.name;
        var lastDot = name.lastIndexOf(".");
        var prefix = lastDot >= 0 ? name.substr(0, lastDot + 1) : "";
        var otherName = prefix + otherProperty;
        return $(element.form).find("[name='" + otherName.replace(/\./g, "\\.") + "']");
    }

    if (value === null || value === undefined || value === "") return true;

    const otherElement = getOtherElement(element, params.other);
    if (otherElement.length === 0) {
        return true;
    }

    const otherVal = otherElement.val() ?? "";

    const checkNormalized = params.checknormalized === true || params.checknormalized === "true";
    if (checkNormalized) {
        return (value + "").toLocaleLowerCase('en-US') !== (otherVal + "").toLocaleLowerCase('en-US');
    }
    else {
        return (value + "") !== (otherVal + "");
    }
});


$.validator.unobtrusive.adapters.add("differentfrom", ["other", "checknormalized"], function (options) {
    options.rules["differentfrom"] = {
        other: options.params.other,
        checknormalized: options.params.checknormalized
    };
    options.messages["differentfrom"] = options.message;
});
