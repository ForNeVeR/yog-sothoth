module YogSothoth.Program

open System.Configuration

let private maxReconnectCount = 10

let private logMessages store jid password roomJid nickname =
    let rec tryConnect tryNumber =
        async {
            try
                return! Xmpp.connect jid password roomJid nickname
            with
            | e ->
                printfn "Error: %A" e
                do! Async.Sleep 5000

                if tryNumber >= maxReconnectCount then
                    return! tryConnect (tryNumber + 1)
                else
                    return! failwithf "Reconnect count exceeded"
        }

    async {
        let! connection = tryConnect 0
        Xmpp.logMessages store connection
        do! Xmpp.waitForTermination connection
    }

let private setting (name : string) =
    ConfigurationManager.AppSettings.[name]

[<EntryPoint>]
let main _ =
    let jid = setting "JID"
    let password = setting "Password"
    let roomJid = setting "RoomJID"
    let nickname = setting "RoomNickname"
    let dataDirectory = setting "DataDirectory"

    use store = Storage.initializeStore dataDirectory
    Async.Start (logMessages store jid password roomJid nickname)
    WebServer.run store
    0
