module YogSothoth.WebServer

open Suave

open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors

let app store =
    let roomMessages room =
        let messages = Storage.getMessages store room
        let response = sprintf "room %s : %d" room messages.Count
        OK response

    choose [ GET >=> choose [ path "/" >=> file "index.html"; browseHome
                              pathScan "/api/messages/%s" roomMessages ]
             NOT_FOUND "Found no handlers." ]

let run store =
    startWebServer defaultConfig (app store)
