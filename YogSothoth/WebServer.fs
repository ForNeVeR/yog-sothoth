module YogSothoth.WebServer

open Suave

let run () =
    startWebServer defaultConfig (Successful.OK "Hello World!")
