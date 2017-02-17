var socket = new WebSocket('ws://localhost:8080/testhub');
var functions = {}

socket.onopen = function () {
    alert('connected');
};

socket.onclose = function () {
    alert('disconnected');
};

socket.onmessage = function (event) {
    processMessage(event.data);
};

function processMessage(message) {
    var parsedMessage = JSON.parse(message);
    if (parsedMessage.setId) {
        var id = parsedMessage.setId;
        localStorage.setItem('webSocketAppId', id);
        var response = { Result: "IsSet", Id: id }
        socket.send(JSON.stringify(response));
        alert(localStorage.getItem('webSocketAppId', id));
    } else if (parsedMessage.callFunction) {
        functions[parsedMessage.callFunction].apply({}, parsedMessage.parameters);
    } else {
        alert(message);
    }
}

//test

functions.test = function(a, b, c) {
    alert("test function is triggered" + a + c + b);
}


window.onload = function() {
    var button = document.getElementById("test-button");
    button.addEventListener('click',
        (event) => {
            var message = document.getElementById("test-input").value;
            socket.send(unescape(encodeURIComponent(JSON.stringify({ hubName: "testhub", callFunction: "testfunction", parameters: [1, "два", message] }))));
        });
}