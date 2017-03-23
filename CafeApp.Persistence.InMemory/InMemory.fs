module InMemory

open Domain 
open System
open System.Collections.Generic
open ReadModel
open Projections
open Table
open Chef
open Waiter
open Cashier
open EventStore
open NEventStore
open Queries

type InMemoryEventStore () =
    static member Instance =
                    Wireup.Init()
                        .UsingInMemoryPersistence()
                        .Build()

let inMemoryEventStore () =
    let eventStoreInstance = InMemoryEventStore.Instance
    {
        GetState = getState eventStoreInstance
        SaveEvent = saveEvents eventStoreInstance
    }

let toDoQueries = {
    GetChefToDos = getChefToDos
    GetCashierToDos = getCashierToDos
    GetWaiterToDos = getWaiterToDos
}

let inMemoryQueries = {
    Table = tableQueries
    ToDo = toDoQueries
}

let inMemoryActions = {
    Table = tableActions
    Chef = chefActions
    Waiter = waiterActions
    Cashier = cashierActions
}