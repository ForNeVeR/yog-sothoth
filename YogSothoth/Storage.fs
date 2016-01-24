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

let private messageLimit = 100

let initializeStore (dataDirectory : string) : IDocumentStore =
    let store = new EmbeddableDocumentStore (DataDirectory = dataDirectory)
    store.Initialize ()

let getRooms (store : IDocumentStore) : ResizeArray<string> =
    use session = store.OpenSession ()
    query {
        for message in session.Query<Message> () do
        select message.Conference
        distinct
    } |> Enumerable.ToList

let getMessages (store : IDocumentStore)
                (room : string)
                (start : DateTime)
                (finish : DateTime) : ResizeArray<Message> =
    use session = store.OpenSession ()
    query {
        for message in session.Query<Message> () do
        where (message.Conference = room && message.DateTime > start && message.DateTime <= finish)
        sortBy message.DateTime
        take messageLimit
    } |> Enumerable.ToList

let save (store : IDocumentStore) (message : Message) : Unit =
    use session = store.OpenSession ()
    session.Store message
    session.SaveChanges ()
