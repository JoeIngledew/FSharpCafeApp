﻿module CommandApi

open System.Text
open CommandHandlers
open OpenTab
open PlaceOrder
open ServeDrink
open PrepareFood
open ServeFood
open CloseTab
open Queries
open Chessie.ErrorHandling

// ValidationQueries -> EventStore -> string
//      -> Async<Result<(State*Event),ErrorResponse>>
let handleCommandRequest queries eventStore = function
| OpenTabRequest tab ->
    queries.Table.GetTableByTableNumber
    |> openTabCommander
    |> handleCommand eventStore tab
| PlaceOrderRequest po ->
    placeOrderCommander queries
    |> handleCommand eventStore po
| ServeDrinkRequest (tabId, drinkMenuNumber) ->
    queries.Drink.GetDrinkByMenuNumber
    |> serveDrinkCommander queries.Table.GetTableByTabId
    |> handleCommand eventStore (tabId, drinkMenuNumber)
| PrepareFoodRequest (tabId, foodMenuNumber) ->
    queries.Food.GetFoodByMenuNumber
    |> prepareFoodCommander queries.Table.GetTableByTabId
    |> handleCommand eventStore (tabId, foodMenuNumber)
| ServeFoodRequest (tabId, foodMenuNumber) ->
    queries.Food.GetFoodByMenuNumber
    |> serveFoodCommander queries.Table.GetTableByTabId
    |> handleCommand eventStore (tabId, foodMenuNumber)
| CloseTabRequest (tabId, amount) ->
    closeTabCommander queries.Table.GetTableByTabId
    |> handleCommand eventStore (tabId, amount)
| _ -> err "Invalid command" |> fail |> async.Return