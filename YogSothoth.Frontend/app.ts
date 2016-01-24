declare var fetch;

function formatDate(date: Date) {
    function padLeft(length, char, s) {
        while (s.toString().length < length) {
            s = char + s.toString();
        }

        return s;
    }

    var year = padLeft(4, '0', date.getUTCFullYear());
    var month = padLeft(2, '0', date.getUTCMonth() + 1);
    var day = padLeft(2, '0', date.getUTCDate());
    var hour = padLeft(2, '0', date.getUTCHours());
    var minute = padLeft(2, '0', date.getUTCMinutes());
    var second = padLeft(2, '0', date.getUTCSeconds());
    return `${year}-${month}-${day}T${hour}:${minute}:${second}`;
}

function getRooms() {
    var url = 'api/rooms';
    return fetch(url).then(response => response.json());
}

function getMessages(room, date) {
    var url = `api/messages/${room}?date=${formatDate(date)}`;
    return fetch(url).then(response => response.json());
}

function cleanElement(element: HTMLElement) {
    while (element.firstChild) {
        element.removeChild(element.firstChild);
    }
}

function enablePolling(settings) {
    function poll() {
        var room = settings.room;
        var date = settings.date;
        getMessages(room, date).then(messages => {
            if (settings.room !== room) {
                setTimeout(poll, 1000);
                return;
            }

            var lastMessage = messages[messages.length - 1];
            if (lastMessage != null) {
                settings.date = new Date(lastMessage.dateTime);
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

function today() {
    var now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), now.getDate());
}

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
