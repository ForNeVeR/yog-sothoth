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
let server = prompt "Server"

let jid = new JID (userName)
let roomJid = new JID (roomName, server, "Yog-Sothoth")
let client = new JabberClient (User = jid.User,
                               Server = jid.Server,
                               Password = password)

client.OnAuthError.Add (fun _ -> printf "Auth error.\n")
client.OnConnect.Add (fun _ -> printf "Connected.\n")
client.OnError.Add (fun e -> printf "%s\n" (e.ToString ()))

let roomManager = new ConferenceManager (Stream = client)
roomManager.add_OnJoin (fun room ->
    printf "Joined to room.\n"
    room.PublicMessage "Hortha Hell!")

client.OnAuthenticate.Add (fun _ ->
    let room = roomManager.GetRoom (roomJid)
    room.Join ())

client.Connect ()

Console.ReadLine () |> ignore
