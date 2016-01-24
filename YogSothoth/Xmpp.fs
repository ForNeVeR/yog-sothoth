module YogSothoth.Xmpp

open System
open System.Threading.Tasks
open System.Xml

open jabber
open jabber.client
open jabber.connection
open Raven.Client

open YogSothoth.Storage

type XmppConnection =
    { Client : JabberClient
      RoomJid : JID }

let private debug = false

let private logInfo text =
    printfn "%s" text

let private logError text =
    let oldColor = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.Red
    logInfo text
    Console.ForegroundColor <- oldColor

let private logElement logger header (element : XmlElement) =
    logger (sprintf "%s: %s" header (element.ToString ()))

let private logElementError = logElement logError
let private logElementInfo = logElement logInfo

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
        client.OnReadText.Add (logInfo << (sprintf "Read: %s"))
        client.OnWriteText.Add (logInfo << (sprintf "Write: %s"))

    client.OnConnect.Add (fun _ -> logInfo "Connected")

    client.Connect ()

    async {
        let completionSource = TaskCompletionSource ()
        client.OnAuthenticate.Add (fun _ -> completionSource.SetResult ())

        do! Async.AwaitTask completionSource.Task
        return
            { Client = client
              RoomJid = roomJid }
    }

let isHistorical (message : jabber.protocol.client.Message) = not (isNull message.["delay"])
let isTechnical (message : jabber.protocol.client.Message) = isNull message.From.Resource

let logMessages (storage : IDocumentStore) { Client = client; RoomJid = roomJid } : Unit =
    let processMessage m =
        if not (isHistorical m || isTechnical m) then
            logElementInfo "Message" m
            let message =
                { Conference = roomJid.BareJID.ToString ()
                  Sender = m.From.Resource
                  DateTime = DateTime.UtcNow
                  Text = m.Body }
            Storage.save storage message

    let roomManager = new ConferenceManager (Stream = client)
    roomManager.add_OnJoin (fun room -> logInfo (sprintf "Joined room %A" room.JID))
    let room = roomManager.GetRoom roomJid
    room.OnRoomMessage.Add processMessage
    room.Join ()

let waitForTermination { Client = client } : Async<Unit> =
    let completionSource = TaskCompletionSource ()
    client.OnDisconnect.Add (fun _ -> completionSource.SetResult ())
    Async.AwaitTask completionSource.Task
