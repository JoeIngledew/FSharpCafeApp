module Cashier

open Domain 
open System
open System.Collections.Generic
open ReadModel
open Projections
open Table

let private cashierTodos = new Dictionary<Guid, Payment> ()

let private addTabAmount tabId amount =
    match getTableByTabId tabId with
    | Some table -> 
        let payment =
            { Tab = {Id = tabId; TableNumber = table.Number}; Amount = amount}
        cashierTodos.Add(tabId, payment)
    | None -> ()
    async.Return ()

let private remove tabId =
    cashierTodos.Remove(tabId) |> ignore
    async.Return ()

let cashierActions = {
    AddTabAmount = addTabAmount
    Remove = remove }

let getCashierToDos () =
    cashierTodos.Values
    |> Seq.toList
    |> async.Return