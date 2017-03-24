module Program

open System
open System.Reflection
open Suave
open Suave.Web
open Suave.Successful
open Suave.RequestErrors
open Suave.Operators
open Suave.Filters
open CommandApi
open InMemory
open System.Text
open Chessie.ErrorHandling
open Events
open Projections
open JsonFormatter
open QueriesApi
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
open System.IO

let eventsStream = new Control.Event<Event list>()

let project event =
    projectedReadModel inMemoryActions event
    |> Async.RunSynchronously |> ignore

let projectEvents = List.iter project

let commandApiHandler eventStore (context : HttpContext) = async {
    let payload =
        Encoding.UTF8.GetString context.request.rawForm
    let! response =
        handleCommandRequest inMemoryQueries eventStore payload
    match response with
    | Ok ((state,events),_) ->
        do! eventStore.SaveEvent state events
        eventsStream.Trigger(events)
        return! toStateJson state context
    | Bad (err) ->
        return! toErrorJson err.Head context
}

let commandApi eventStore =
    path "/command"
        >=> POST
        >=> commandApiHandler eventStore

let socketHandler (ws : WebSocket) (cx : HttpContext) =
    socket {
        while true do
            let! events =
                Control.Async.AwaitEvent(eventsStream.Publish)
                |> Suave.Sockets.SocketOp.ofAsync
            for event in events do
                let eventData =
                    event |> eventJObj |> string |> Encoding.UTF8.GetBytes
                    |> ByteSegment
                do! ws.send Text eventData true
    }

let clientDir =
    let exePath = Assembly.GetEntryAssembly().Location
    let exeDir = (new FileInfo(exePath)).Directory
    Path.Combine(exeDir.FullName, "public")

[<EntryPoint>]
let main argv =
    let app =
        let eventStore = inMemoryEventStore ()
        choose [
            path "/websocket" >=>
                handShake socketHandler
            commandApi eventStore
            queriesApi inMemoryQueries eventStore
            GET >=> choose [
                path "/" >=> Files.browseFileHome "index.html"
                Files.browseHome
            ]
        ]
    let cfg =
        { defaultConfig with
            homeFolder = Some(clientDir)
            bindings = [HttpBinding.createSimple HTTP "0.0.0.0" 8083] }
    startWebServer cfg app
    eventsStream.Publish.Add(projectEvents)
    0 // return an integer exit code
