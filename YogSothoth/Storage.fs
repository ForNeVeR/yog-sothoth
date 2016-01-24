module YogSothoth.Storage

open System
open System.Linq
open System.Runtime.Serialization

open Raven.Client
open Raven.Client.Embedded

[<DataContract>]
type Message =
    { [<field: DataMember(Name = "conference")>]
      Conference : string

      [<field: DataMember(Name = "sender")>]
      Sender : string

      DateTime : DateTime

      [<field: DataMember(Name = "text")>]
      Text : string }
    with
        [<DataMember(Name = "dateTime")>]
        member this.SerializableDateTime
            with get () = this.DateTime.ToString "s"
            and set (_ : string) = ()

let private messageLimit = 100

let private createQuery<'T> (session : IDocumentSession) =
    session.Query<'T>().Customize(fun x -> ignore (x.WaitForNonStaleResultsAsOfLastWrite ()))

let initializeStore (dataDirectory : string) : IDocumentStore =
    let store = new EmbeddableDocumentStore (DataDirectory = dataDirectory)
    store.Initialize ()

let getRooms (store : IDocumentStore) : ResizeArray<string> =
    use session = store.OpenSession ()
    query {
        for message in session.Query<Message>() do
        select message.Conference
        distinct
    } |> Enumerable.ToList

let getMessages (store : IDocumentStore)
                (room : string)
                (start : DateTime)
                (finish : DateTime) : ResizeArray<Message> =
    use session = store.OpenSession ()
    query {
        for message in createQuery session do
        where (message.Conference = room && message.DateTime > start && message.DateTime <= finish)
        sortBy message.DateTime
        take messageLimit
    } |> Enumerable.ToList

let save (store : IDocumentStore) (message : Message) : Unit =
    use session = store.OpenSession ()
    session.Store message
    session.SaveChanges ()
