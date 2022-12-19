const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifications")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start();
        document.querySelector("#notifications").innerHTML += `<li>SignalR Connected</li>`;``;
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

connection.on("SendAsync", (user, message) => {
    document.querySelector("#notifications").innerHTML += `<li>${message}</li>`;``;
});

connection.onclose(async () => {
    await start();
});

// Start the connection.
start();