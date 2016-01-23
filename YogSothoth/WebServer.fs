module YogSothoth.WebServer

open System

open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors

let private date year month day =
    DateTime (year, month, day, 0, 0, 0, DateTimeKind.Utc)

let private app store =
    let roomMessages (room, year, month, day) =
        let start = date year month day
        let finish = start.AddDays 1.0
        let messages = Storage.getMessages store room start finish
        let response = sprintf "room %s : %d" room messages.Count
        OK response

    choose [ GET >=> choose [ path "/" >=> file "index.html"; browseHome
                              pathScan "/api/messages/%s/%d/%d/%d" roomMessages ]
             NOT_FOUND "Found no handlers." ]

let run store =
    startWebServer defaultConfig (app store)
