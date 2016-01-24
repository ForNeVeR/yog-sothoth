module YogSothoth.WebServer

open System
open System.Globalization
open System.Runtime.Serialization

open Suave
open Suave.Embedded
open Suave.Filters
open Suave.Json
open Suave.Operators
open Suave.RequestErrors
open Suave.Successful

[<DataContract>]
type Message =
    { [<field: DataMember(Name = "sender")>]
      Sender : string

      [<field: DataMember(Name = "dateTime")>]
      DateTime : string

      [<field: DataMember(Name = "text")>]
      Text : string }

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
        let messages =
            Storage.getMessages store room startDate finish
            |> Seq.map (fun { Sender = sender
                              DateTime = dateTime
                              Text = text } -> { Sender = sender
                                                 DateTime = dateTime.ToString "s"
                                                 Text = text })
            |> Seq.toArray
        okJson messages

    let getRooms =
        request (fun _ ->
            let rooms = Storage.getRooms store
            okJson rooms)

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
