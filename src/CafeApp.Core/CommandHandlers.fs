module CommandHandlers

open States
open Events
open Commands
open Domain
open Chessie.ErrorHandling
open Errors

let payment order = {Tab = order.Tab; Amount = orderAmount order}

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

let (|ServeDrinkCompletesIPOrder|_|) ipo drink =
    match isServingDrinkCompletesIPOrder ipo drink with
    | true -> Some drink
    | false -> None

let validHandleServeDrink drink order =
    let event = DrinkServed (drink,order.Tab.Id)
    match drink with 
    | NonOrderedDrink order _ ->
        CanNotServeNonOrderedDrink drink |> fail
    | ServeDrinkCompletesOrder order _ ->
        let payment = {Tab = order.Tab; Amount = orderAmount order}
        event :: [OrderServed (order, payment)] |> ok
    | _ -> [event] |> ok

let handleServeDrink drink tabId = function
| PlacedOrder order ->
    validHandleServeDrink drink order
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotServeWithClosedTab |> fail
| ModifiedOrder order -> 
    validHandleServeDrink drink order
| OrderInProgress ipo ->
    let order = ipo.PlacedOrder
    let drinkServed = DrinkServed (drink, order.Tab.Id)
    match drink with
    | NonOrderedDrink order _ ->
        CanNotServeNonOrderedDrink drink |> fail
    | AlreadyServedDrink ipo _ ->
        CanNotServeAlreadyServedDrink drink |> fail
    | ServeDrinkCompletesIPOrder ipo _ ->
        
        drinkServed :: [OrderServed (order, payment order)]
        |> ok
    | _ -> [drinkServed] |> ok

let (|NonOrderedFood|_|) order food =
    match List.contains food order.Foods with 
    | false -> Some food
    | true -> None

let (|AlreadyPreparedFood|_|) ipo food =
    match List.contains food ipo.PreparedFoods with
    | true -> Some food
    | false -> None

let (|ServeFoodCompletesIPOrder|_|) ipo food =
    match isServindFoodCompletesIPOrder ipo food with
    | true -> Some food
    | false -> None

let validHandlePrepareFood food order =
    match food with
    | NonOrderedFood order _ ->
        CanNotPrepareNonOrderedFood food |> fail
    | _ -> [FoodPrepared (food, order.Tab.Id)] |> ok

let handlePrepareFood food tabId = function
| PlacedOrder order ->
    validHandlePrepareFood food order
| ModifiedOrder order ->
    validHandlePrepareFood food order
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ -> CanNotPrepareForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotPrepareWithClosedTab |> fail
| OrderInProgress ipo ->
    let order = ipo.PlacedOrder
    let foodServed = FoodServed (food, tabId)
    match food with 
    | NonOrderedFood order _ ->
        CanNotPrepareNonOrderedFood food |> fail
    | AlreadyPreparedFood ipo _ ->
        CanNotPrepareAlreadyPreparedFood food |> fail
    | _ -> [FoodPrepared (food, tabId)] |> ok

let (|UnPreparedFood|_|) ipo food =
    match List.contains food ipo.PreparedFoods with
    | false -> Some food
    | true -> None

let (|AlreadyServedFood|_|) ipo food =
    match List.contains food ipo.ServedFoods with
    | true -> Some food
    | false -> None

let handleServeFood food tabId = function
| OrderInProgress ipo ->
    let order = ipo.PlacedOrder
    let foodServed = FoodServed (food, tabId)
    match food with
    | NonOrderedFood order _ ->
        CanNotServeNonOrderedFood food |> fail
    | AlreadyServedFood ipo _ ->
        CanNotServeAlreadyServedFood food |> fail
    | UnPreparedFood ipo _ ->
        CanNotServeNonPreparedFood food |> fail
    | ServeFoodCompletesIPOrder ipo _ ->
        foodServed :: [OrderServed (ipo.PlacedOrder, payment order)]
        |> ok
    | _ -> [FoodServed (food, tabId)] |> ok
| PlacedOrder _ -> CanNotServeNonPreparedFood food |> fail
| ModifiedOrder _ -> CanNotServeNonPreparedFood food |> fail
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotServeWithClosedTab |> fail

let handleCloseTab payment = function
| ServedOrder order ->
    let orderAmount = orderAmount order
    if payment.Amount >= orderAmount then 
        [TabClosed payment] |> ok
    else 
        InvalidPayment (orderAmount, payment.Amount) |> fail
| _ -> CanNotPayForNonServedOrder |> fail

let handleModifyOrder order = function
| PlacedOrder oldOrder ->
    [OrderModified order] |> ok
| _ -> failwith "todo"

let execute state command =
    match command with
    | OpenTab tab -> handleOpenTab tab state
    | PlaceOrder order -> handlePlaceOrder order state
    | ServeDrink (drink,tabId) -> handleServeDrink drink tabId state
    | PrepareFood (food, tabId) -> handlePrepareFood food tabId state
    | ServeFood (food, tabId) -> handleServeFood food tabId state
    | CloseTab payment -> handleCloseTab payment state
    | ModifyOrder order -> handleModifyOrder order state

let evolve state command =
    match execute state command with
    | Ok (events,_) ->
        let newState = List.fold States.apply state events
        (newState, events) |> ok
    | Bad err -> Bad err