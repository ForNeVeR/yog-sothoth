module YogSothoth.Program

open System.Configuration

let private maxReconnectCount = 10

let private logMessages jid password roomJid nickname =
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
        Xmpp.logMessages connection
        do! Xmpp.waitForTermination connection
        return 0
    }


[<EntryPoint>]
let main _ =
    let jid = ConfigurationManager.AppSettings.["JID"]
    let password = ConfigurationManager.AppSettings.["Password"]
    let roomJid = ConfigurationManager.AppSettings.["RoomJID"]
    let nickname = ConfigurationManager.AppSettings.["RoomNickname"]

    Async.RunSynchronously (logMessages jid password roomJid nickname)
