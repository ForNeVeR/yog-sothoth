declare var fetch;

// Date functions
function today() {
    var now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), now.getDate());
}

function tommorow(date: Date) {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1);
}

// DOM functions
function cleanElement(element: HTMLElement) {
    while (element.firstChild) {
        element.removeChild(element.firstChild);
    }
}

// API functions
function getRooms() {
    var url = 'api/rooms';
    return fetch(url).then(response => response.json());
}

function getMessages(room, from, to) {
    var url = `api/messages/${room}?from=${from.getTime()}&to=${to.getTime()}`;
    return fetch(url).then(response => response.json());
}

// Business logic functions
function enablePolling(settings) {
    function poll() {
        var room = settings.room;
        var date = settings.date;
        var to = tommorow(date);
        getMessages(room, date, to).then(messages => {
            if (settings.room !== room) {
                setTimeout(poll, 1000);
                return;
            }

            var lastMessage = messages[messages.length - 1];
            if (lastMessage != null) {
                settings.date = new Date(lastMessage.timestamp);
            }

            messages.forEach(message => {
                var container = document.createElement('ul');
                var name = document.createElement('span');
                var body = document.createElement('span');

                name.innerText = message.sender;
                body.innerText = message.text;
                container.appendChild(name);
                container.appendChild(body);
                settings.element.appendChild(container);
            });

            setTimeout(poll, 1000);
        });
    }

    setTimeout(poll, 1000);
}

// Endpoint
window.onload = () => {
    var room = <HTMLSelectElement>document.getElementById('room');
    var messages = document.getElementById('messages');
    var pollSettings = {
        element: messages,
        room: '',
        date: today()
    };

    room.addEventListener('change', () => {
        var value = room.value;
        if (value !== pollSettings.room) {
            cleanElement(messages);
            pollSettings.room = value;
            pollSettings.date = new Date(2016, 0, 24, 13, 0, 0)// today();
        }
    });

    getRooms().then(rooms => {
        cleanElement(room);
        rooms.forEach(roomName => {
            var element = document.createElement('option');
            element.innerText = roomName;
            element.value = roomName;
            room.appendChild(element);
        });
    });

    enablePolling(pollSettings);
};
