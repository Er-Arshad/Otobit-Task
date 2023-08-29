// mySignalRScript.js

// Import the SignalR library from the CDN
//import * as signalR from 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.10/dist/browser/signalr.min.js';
//import * as jquery from 'http://ajax.microsoft.com/ajax/jquery/jquery-1.10.2.js';
var connection = new signalR.HubConnectionBuilder().withUrl("/Notification").build();
jquery.alert(connection);
jquery.alert("Hlloe3");
// Your JavaScript code that uses SignalR can go here
