module YogSothoth.Storage

open System

open Raven.Client
open Raven.Client.Embedded

type Message =
    { Conference : string
      Sender : string
      DateTime : DateTime
      Text : string }

let initializeStore (dataDirectory : string) : IDocumentStore =
    let store = new EmbeddableDocumentStore (DataDirectory = dataDirectory)
    store.Initialize ()

let save (store : IDocumentStore) (message : Message) : Unit =
    use session = store.OpenSession ()
    session.Store message
    session.SaveChanges ()
