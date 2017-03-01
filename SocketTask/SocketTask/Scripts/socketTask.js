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
    //Id setting
    if (parsedMessage.setId) {
        var id = parsedMessage.setId;
        localStorage.setItem('webSocketAppId', id);
        var response = { Result: "IsSet", Id: id }
        socket.send(JSON.stringify(response));
        alert(localStorage.getItem('webSocketAppId', id));
    //Getting hub names
    } else if (parsedMessage.HubNames) {
        var hubNames = { HubNames: ["testHub", "testHub2"] };
        socket.send(JSON.stringify(hubNames));
    //Call function 
    } else if (parsedMessage.functionName) {
        functions[parsedMessage.functionName].apply({}, parsedMessage.parameters);
    } else {
        alert(message);
    }
}

//test

functions.test = function(a, b, c) {
    alert("test function is triggered" + a + c + b);
}

functions.write = function(v) {
    alert(v);
}


window.onload = function() {
    var button = document.getElementById("test-button");
    button.addEventListener('click',
        (event) => {
            var message = document.getElementById("test-input").value;
            socket.send(unescape(encodeURIComponent(JSON.stringify({ hubName: "testHub2", functionName: "TestFunction", parameters: [1, "два", message] }))));
        });
    var writeButton = document.getElementById("write-button");
    writeButton.addEventListener('click',
        (event) => {
            var message = document.getElementById("write-input").value;
            socket.send(unescape(encodeURIComponent(JSON.stringify({ hubName: "testHub", functionName: "ThisIsForReal", parameters: [message] }))));
        });
}