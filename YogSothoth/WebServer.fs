module YogSothoth.WebServer

open System
open System.Globalization

open Suave
open Suave.Embedded
open Suave.Filters
open Suave.Json
open Suave.Operators
open Suave.RequestErrors
open Suave.Successful

let private isoDateFormat = "yyyy-MM-ddTHH:mm:ss"

let private parseDate date =
    DateTime.ParseExact (date,
                         isoDateFormat,
                         CultureInfo.InvariantCulture,
                         DateTimeStyles.AssumeUniversal ||| DateTimeStyles.AdjustToUniversal)

let private okJson o =
    let json = toJson o
    ok json

let private app store =
    let getRoomMessages room (startDate : DateTime) =
        let finish = startDate.Date.AddDays 1.0
        let messages = Storage.getMessages store room startDate finish
        okJson messages

    let getRooms =
        let rooms = Storage.getRooms store
        okJson rooms

    let roomMessagesHandler room =
        request (fun r -> cond (r.queryParam "date") (parseDate >> getRoomMessages room) never)

    choose [ GET >=> choose [ path "/" >=> resourceFromDefaultAssembly "index.html"
                              path "/app.js" >=> resourceFromDefaultAssembly "app.js"
                              path "/app.css" >=> resourceFromDefaultAssembly "app.css"
                              path "/api/rooms" >=> getRooms
                              pathScan "/api/messages/%s" roomMessagesHandler ]
             NOT_FOUND "Found no handlers." ]

let run store =
    startWebServer defaultConfig (app store)
