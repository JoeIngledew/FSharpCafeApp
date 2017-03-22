module CommandHandlers

open States
open Events
open Commands
open System
open Domain
open Chessie.ErrorHandling
open Errors

let handleOpenTab tab = function
| ClosedTab _ -> [TabOpened tab] |> ok
| _ -> TabAlreadyOpened |> fail

let handlePlaceOrder order = function
| OpenedTab _ -> [OrderPlaced order] |> ok
| _ -> failwith "todo"

let execute state command =
  match command with
  | OpenTab tab -> handleOpenTab tab state
  | PlaceOrder order -> handlePlaceOrder order state
  | _ -> failwith "ToDo"

let evolve state command =
  match execute state command with
  | Ok (events,_) ->
    let newState = List.fold States.apply state events
    (newState, events) |> ok
  | Bad err -> Bad err