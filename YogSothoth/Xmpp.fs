module YogSothoth.Xmpp

open System
open System.Threading.Tasks
open System.Xml

open jabber
open jabber.client
open jabber.connection

type XmppConnection =
    { Client : JabberClient
      RoomJid : JID }

let private debug = false

let private logInfo message =
    printfn "%s" message

let private logError = logInfo

let private logElement logger header (element : XmlElement) =
    logger (sprintf "%s: %s" header (element.ToString ()))

let private logElementError = logElement logError

let connect (jid : string) (password : string) (roomJid : string) (nickname : string) : Async<XmppConnection> =
    let jid = JID jid
    let roomJid = JID roomJid
    let roomJid = JID (roomJid.User, roomJid.Server, nickname)
    let client = new JabberClient (User = jid.User,
                                   Server = jid.Server,
                                   Password = password)

    client.add_OnInvalidCertificate (fun _ _ _ _ -> true)

    client.OnAuthError.Add (logElementError "Auth error")
    client.OnError.Add (fun e -> logError (sprintf "Error: %s" (e.ToString ())))

    if debug then
        client.OnReadText.Add (fun text -> logInfo (sprintf "Read: %s" text))
        client.OnWriteText.Add (fun text -> logInfo (sprintf "Write: %s" text))

    client.OnConnect.Add (fun _ -> logInfo "Connected")

    client.Connect ()

    async {
        let completionSource = TaskCompletionSource()
        client.OnAuthenticate.Add (fun _ -> completionSource.SetResult ())

        do! Async.AwaitTask completionSource.Task
        return
            { Client = client
              RoomJid = roomJid }
    }

let logMessages { Client = client; RoomJid = roomJid } : Unit =
    let roomManager = new ConferenceManager (Stream = client)
    roomManager.add_OnJoin (fun room -> logInfo (sprintf "Joined room %A" room.JID))
    let room = roomManager.GetRoom roomJid
    room.Join ()

let waitForTermination { Client = client } : Async<Unit> =
    let completionSource = TaskCompletionSource()
    client.OnDisconnect.Add (fun _ -> completionSource.SetResult ())
    Async.AwaitTask completionSource.Task
