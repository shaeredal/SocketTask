'use strict';

angular.module('webSockets', [])
.run(function ($timeout) {

    var socket = new WebSocket('ws://localhost:8080/ws');

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
        } else {
            alert(message);
        }
    }

    $timeout(function() {
        var button = document.getElementById("test-button");
        button.addEventListener('click',
            (event) => {
                var message = document.getElementById("test-input").value;
                socket.send(unescape(encodeURIComponent(message)));
            });
    }, 1000);
})