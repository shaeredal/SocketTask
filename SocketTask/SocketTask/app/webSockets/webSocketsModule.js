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
        alert("Get data: " + event.data);
    };

    $timeout(function() {
        var button = document.getElementById("test-button");
        button.addEventListener('click',
            (event) => {
                var message = document.getElementById("test-input").value;
                socket.send(unescape(encodeURIComponent(message)));
            });
    }, 1000);
})