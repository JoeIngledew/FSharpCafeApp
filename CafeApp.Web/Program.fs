﻿module Program

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
        return! OK (sprintf "%A" state) context
    | Bad (err) ->
        return! BAD_REQUEST err.Head.Message context
}

let commandApi eventStore =
    path "/command"
        >=> POST
        >=> commandApiHandler eventStore

[<EntryPoint>]
let main argv = 
    let app =
        let eventStore = inMemoryEventStore ()
        choose [
            commandApi eventStore
        ]
    let cfg =
        { defaultConfig with bindings = [HttpBinding.createSimple HTTP "0.0.0.0" 8083] }
    startWebServer cfg app
    eventsStream.Publish.Add(projectEvents)
    0 // return an integer exit code
