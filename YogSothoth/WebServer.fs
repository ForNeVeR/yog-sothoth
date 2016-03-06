module YogSothoth.WebServer

open System.IO
open System.Runtime.Serialization

open Raven.Client
open Suave
open Suave.Files
open Suave.Filters
open Suave.Json
open Suave.Operators
open Suave.RequestErrors
open Suave.Successful

[<DataContract>]
type Message =
    { [<field: DataMember(Name = "sender")>]
      Sender : string

      [<field: DataMember(Name = "timestamp")>]
      Timestamp : int64

      [<field: DataMember(Name = "text")>]
      Text : string }

let private okJson o =
    let json = toJson o
    ok json

let private app contentDirectory store =
    let withTimestamps func (request : HttpRequest) =
        cond (request.queryParam "from") (fun fromValue ->
            let from = int64 fromValue
            cond (request.queryParam "to") (fun toValue ->
                let ``to`` = int64 toValue
                func (from, ``to``)) never) never

    let getRoomMessages room (startDate, endDate) =
        let messages =
            Storage.getMessages store room startDate endDate
            |> Seq.map (fun { Sender = sender
                              Timestamp = timestamp
                              Text = text } -> { Sender = sender
                                                 Timestamp = timestamp
                                                 Text = text })
            |> Seq.toArray
        okJson messages

    let getRooms =
        request (fun _ ->
            let rooms = Storage.getRooms store
            okJson rooms)

    let roomMessagesHandler room =
        request (withTimestamps (getRoomMessages room))

    choose [ GET >=> choose [ path "/api/rooms" >=> getRooms
                              pathScan "/api/messages/%s" roomMessagesHandler
                              path "/" >=> file (Path.Combine (contentDirectory, "index.html"))
                              browse contentDirectory ]
             NOT_FOUND "Found no handlers." ]

let run (contentDirectory : string) (store : IDocumentStore) : unit =
    startWebServer defaultConfig (app contentDirectory store)
