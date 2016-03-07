import {Injectable} from 'angular2/core';
import {Room} from './room';

@Injectable()
export class RoomService {
    getRooms(): Promise<Room[]> {
        return fetch('api/rooms')
            .then(response => response.json())
            .then(names => {
                console.log('names', names);
                return names.map(name => { return { name: name } });
            });
    }
}
