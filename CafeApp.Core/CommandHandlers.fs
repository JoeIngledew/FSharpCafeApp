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
| OpenedTab _ -> 
    if List.isEmpty order.Foods && List.isEmpty order.Drinks then
        fail CanNotPlaceEmptyOrder
    else
        [OrderPlaced order] |> ok
| ClosedTab _ -> fail CanNotOrderWithClosedTab
| _ -> fail OrderAlreadyPlaced

let (|NonOrderedDrink|_|) order drink =
    match List.contains drink order.Drinks with
    | false -> Some drink
    | true -> None

let (|ServeDrinkCompletesOrder|_|) order drink =
    match isServingDrinkCompletesOrder order drink with
    | true -> Some drink
    | false -> None

let (|AlreadyServedDrink|_|) ipo drink =
    match List.contains drink ipo.ServedDrinks with
    | true -> Some drink
    | false -> None

let handleServeDrink drink tabId = function
| PlacedOrder order ->
    let event = DrinkServed (drink,tabId)
    match drink with 
    | NonOrderedDrink order _ ->
        CanNotServeNonOrderedDrink drink |> fail
    | ServeDrinkCompletesOrder order _ ->
        let payment = {Tab = order.Tab; Amount = orderAmount order}
        event :: [OrderServed (order, payment)] |> ok
    | _ -> [event] |> ok
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotServeWithClosedTab |> fail
| OrderInProgress ipo ->
    let order = ipo.PlacedOrder
    let drinkServed = DrinkServed (drink, order.Tab.Id)
    match drink with
    | NonOrderedDrink order _ ->
        CanNotServeNonOrderedDrink drink |> fail
    | AlreadyServedDrink ipo _ ->
        CanNotServeAlreadyServedDrink drink |> fail
    | _ -> [drinkServed] |> ok

let (|NonOrderedFood|_|) order food =
    match List.contains food order.Foods with 
    | false -> Some food
    | true -> None

let (|AlreadyPreparedFood|_|) ipo food =
    match List.contains food ipo.PreparedFoods with
    | true -> Some food
    | false -> None

let handlePrepareFood food tabId = function
| PlacedOrder order ->
    match food with
    | NonOrderedFood order _ ->
        CanNotPrepareNonOrderedFood food |> fail
    | _ -> [FoodPrepared (food, tabId)] |> ok
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ -> CanNotPrepareForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotPrepareWithClosedTab |> fail
| OrderInProgress ipo ->
    let order = ipo.PlacedOrder
    match food with 
    | NonOrderedFood order _ ->
        CanNotPrepareNonOrderedFood food |> fail
    | AlreadyPreparedFood ipo _ ->
        CanNotPrepareAlreadyPreparedFood food |> fail
    | _ -> [FoodPrepared (food, tabId)] |> ok

let execute state command =
  match command with
  | OpenTab tab -> handleOpenTab tab state
  | PlaceOrder order -> handlePlaceOrder order state
  | ServeDrink (drink,tabId) -> handleServeDrink drink tabId state
  | PrepareFood (food, tabId) -> handlePrepareFood food tabId state
  | _ -> failwith "ToDo"

let evolve state command =
  match execute state command with
  | Ok (events,_) ->
    let newState = List.fold States.apply state events
    (newState, events) |> ok
  | Bad err -> Bad err