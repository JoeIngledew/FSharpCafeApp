module CommandHandlers

open States
open Events
open System
open Domain

let execute state command =
  match command with
  | _ -> [TabOpened {Id = Guid.NewGuid(); TableNumber = 1}]

let evolve state command =
  let events = execute state command
  let newState = List.fold apply state events
  (newState,events)
