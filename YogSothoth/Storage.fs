module YogSothoth.Storage

open System
open System.Linq

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

let getMessages (store : IDocumentStore) (room : string) : ResizeArray<Message> =
    use session = store.OpenSession ()
    session.Query<Message>().ToList()

let save (store : IDocumentStore) (message : Message) : Unit =
    use session = store.OpenSession ()
    session.Store message
    session.SaveChanges ()
