(* Copyright (C) 2011 by ForNeVeR

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. *)

module YogSothoth
open System
open jabber
open jabber.client
open jabber.connection

let prompt (message : string) =
    Console.Write (message + ": ")
    Console.ReadLine ()

let userName = prompt "JID"
let password = prompt "Password"
let owner = prompt "Owner"
let roomName = prompt "Room"

let jid = new JID (userName)
let roomJid = new JID (roomName + "/Yog-Sothoth")
let client = new JabberClient (User = jid.User,
                               Server = jid.Server,
                               NetworkHost = jid.Server,
                               Password = password)
client.AutoPresence <- false
let roomManager = new ConferenceManager (Stream = client)

client.OnAuthError.Add (fun _ -> printf "Auth error.")
client.OnConnect.Add (fun _ -> printf "Connected.")
client.OnError.Add (fun e -> printf "%s" (e.ToString ()))
client.OnAuthenticate.Add (fun _ ->
    client.Message (owner, "I'm online!")
    let room = roomManager.GetRoom (roomJid)
    room.add_OnPresenceError (fun _ e -> printf "%s" (e.ToString ()))
    room.add_OnJoin (fun _ ->
        room.PublicMessage ("Hello room!"))
    room.Join ()
)

client.Connect ()

Console.ReadLine () |> ignore
