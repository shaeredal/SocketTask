var app = angular.module('socketTask',
[
    'ngRoute',
    'socketTask.main'
]);

app.config([
    '$routeProvider',
    function($routeProvider) {
        $routeProvider.when('/main',
            {
                templateUrl: '/app/views/main.html',
                controller: 'mainController'
            })
            .otherwise({
                redirectTo: '/main',
                controller: 'mainController'
            });
    }
]);