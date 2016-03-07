import {Component} from 'angular2/core';
import {OnInit} from 'angular2/core';
import {Room} from './room';
import {RoomService} from './room.service';

@Component({
    selector: 'ys-app',
    templateUrl: 'app.component.html',
    providers: [ RoomService ]
})
export class AppComponent implements OnInit {
    constructor(private _roomService: RoomService) {
    }

    rooms: Room[];
    selectedRoomName: string;
    pollSettings = {
        element: HTMLElement = null,
        room: '',
        date: today()
    };

    ngOnInit() {
        const messages = document.getElementById('messages');
        this.pollSettings.element = messages;
        this._roomService.getRooms().then(rooms => {
            this.rooms = rooms;
        });
        enablePolling(this.pollSettings);
    }

    onRoomSelected() {
        const name = this.selectedRoomName;
        if (name !== this.pollSettings.room) {
            cleanElement(this.pollSettings.element);
            this.pollSettings.room = name;
            this.pollSettings.date = today();
        }
    }
}

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
