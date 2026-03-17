window.notificationHelper = {
    requestPermission: async function () {
        if (!("Notification" in window)) {
            console.log("This browser does not support desktop notification");
            return "denied";
        }
        
        let permission = await Notification.requestPermission();
        return permission;
    },
    
    sendNotification: function (title, options) {
        if (Notification.permission === "granted") {
            const notification = new Notification(title, options);
            notification.onclick = function() {
                window.focus();
                this.close();
            };
        }
    }
};
