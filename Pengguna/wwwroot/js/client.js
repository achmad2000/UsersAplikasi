//// client.js (USER)
//console.log("User client.js loaded");

//const connection = new signalR.HubConnectionBuilder()
//    .withUrl("/Hubs/User") // harus sama dengan MapHub di server
//    .build();

//// Saat terkoneksi
//connection.start()
//    .then(() => {
//        console.log("✅ Connected to Hub as USER");
//        // kirim pesan ke admin kalau mau
//        connection.invoke("SendMessageToAdmin", "User connected!");
//    })
//    .catch(err => console.error("❌ SignalR connection failed:", err));

//// Terima pesan dari admin
//connection.on("ReceiveMessageFromAdmin", (message) => {
//    console.log("📩 Dari Admin:", message);

//    const msgArea = document.getElementById("messages");
//    if (msgArea) {
//        msgArea.innerHTML += `<div class='admin-msg'>Admin: ${message}</div>`;
//    }
//});
