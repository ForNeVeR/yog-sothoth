module YogSothoth.WebServer

open System
open System.Globalization

open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors

open YogSothoth.Storage

let private isoDateFormat = "yyyy-MM-ddTHH:mm:ss"

let private parseDate date =
    DateTime.ParseExact (date, isoDateFormat, CultureInfo.InvariantCulture)

let private app store =
    let getRoomMessages room (startDate : DateTime) =
        let finish = startDate.Date.AddDays 1.0
        Storage.getMessages store room startDate finish

    let response room (messages : ResizeArray<Message>) = OK (sprintf "room %s : %d" room messages.Count)
    let roomMessageHandler room =
        request (fun r -> cond (r.queryParam "date") (parseDate >> getRoomMessages room >> response room) never)

    choose [ GET >=> choose [ path "/" >=> file "index.html"; browseHome
                              pathScan "/api/messages/%s" roomMessageHandler ]
             NOT_FOUND "Found no handlers." ]

let run store =
    startWebServer defaultConfig (app store)
