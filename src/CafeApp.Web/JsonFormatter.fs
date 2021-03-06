﻿module JsonFormatter

open Newtonsoft.Json.Linq
open Domain
open States
open Suave
open Suave.Operators
open Suave.Successful
open CommandHandlers
open Suave.RequestErrors
open ReadModel
open Events

let (.=) key (value : obj) = new JProperty(key, value)

let jobj jProperties =
    let jObject = new JObject()
    jProperties |> List.iter jObject.Add
    jObject

let jArray jObjects =
    let jArray = new JArray()
    jObjects |> List.iter jArray.Add
    jArray

let tabIdJObj tabId =
    jobj [
        "tabId" .= tabId
    ]

let tabJObj tab =
    jobj [
        "id" .= tab.Id
        "tableNumber" .= tab.TableNumber
    ]

let itemJObj item =
    jobj [
        "menuNumber" .= item.MenuNumber
        "name" .= item.Name
    ]

let foodJObj (Food item) = itemJObj item
let drinkJObj (Drink item) = itemJObj item

let foodJArray foods =
    foods |> List.map foodJObj |> jArray

let drinkJArray drinks =
    drinks |> List.map drinkJObj

// Define JObject types for Order and InProgressOrder

let orderJObj (order : Order) =
    jobj [
        "tabId" .= order.Tab.Id
        "tableNumber" .= order.Tab.TableNumber
        "foods" .= foodJArray order.Foods
        "drinks" .= drinkJArray order.Drinks
    ]

let inProgressOrderJObj ipo =
    jobj [
        "tabId" .= ipo.PlacedOrder.Tab.Id
        "tableNumber" .= ipo.PlacedOrder.Tab.TableNumber
        "preparedFoods" .= foodJArray ipo.PreparedFoods
        "servedFoods" .= foodJArray ipo.ServedFoods
        "servedDrinks" .= drinkJArray ipo.ServedDrinks
    ]

let stateJObj = function
| ClosedTab tabId ->
    let state = "state" .= "ClosedTab"
    match tabId with
    | Some id ->
        jobj [ state; "data" .= tabIdJObj id ]
    | None -> jobj [state]
| OpenedTab tab ->
    jobj [
        "state" .= "OpenedTab"
        "data" .= tabJObj tab
    ]
| PlacedOrder order ->
    jobj [
        "state" .= "PlacedOrder"
        "data" .= orderJObj order
    ]
| OrderInProgress ipo ->
    jobj [
        "state" .= "OrderInProgress"
        "data" .= inProgressOrderJObj ipo
    ]
| ServedOrder order ->
    jobj [
        "state" .= "ServedOrder"
        "data" .= orderJObj order
    ]
| ModifiedOrder order ->
    jobj [
        "state" .= "ModifiedOrder"
        "data" .= orderJObj order
    ]

let statusJObj = function
| Open tabId ->
    "status" .= jobj [ "open" .= tabId.ToString() ]
| InService tabId ->
    "status" .= jobj [ "inService" .= tabId.ToString() ]
| Closed -> "status" .= "closed"

let tableJObj table =
    jobj [
        "number" .= table.Number
        "waiter" .= table.Waiter
        statusJObj table.Status
    ]

let chefToDoJObj (todo : ChefToDo) =
    jobj [
        "tabId" .= todo.Tab.Id.ToString()
        "tableNumber" .= todo.Tab.TableNumber
        "foods" .= foodJArray todo.Foods
    ]

let waiterToDoJObj (todo : WaiterToDo) =
    jobj [
        "tabId" .= todo.Tab.Id.ToString()
        "tableNumber" .= todo.Tab.TableNumber
        "foods" .= foodJArray todo.Foods
        "drinks" .= drinkJArray todo.Drinks
    ]

let cashierToDoJObj (payment : Payment) =
    jobj [
        "tabId" .= payment.Tab.Id
        "tableNumber" .= payment.Tab.TableNumber
        "pamentAmount" .= payment.Amount
    ]


let JSON webpart jsonString (context : HttpContext) = async {
    let wp =
        webpart jsonString >=>
            Writers.setMimeType "application/json; charset=utf-8"
    return! wp context
}

let toReadModelsJson toJObj key models =
    models
    |> List.map toJObj |> jArray
    |> (.=) key
    |> Seq.singleton
    |> Seq.toList
    |> jobj
    |> string |> JSON OK

let toTablesJson = toReadModelsJson tableJObj "tables"

let toChefToDosJson =
    toReadModelsJson chefToDoJObj "chefToDos"

let toWaiterToDosJson =
    toReadModelsJson waiterToDoJObj "waiterToDos"

let toCashierToDosJson =
    toReadModelsJson cashierToDoJObj "cashierToDos"

let toFoodsJson =
    toReadModelsJson foodJObj "foods"

let toDrinksJson =
    toReadModelsJson drinkJObj "drinks"

let toStateJson state =
    state |> stateJObj |> string |> JSON OK

let toErrorJson err =
    jobj [ "error" .= err.Message ]
    |> string |> JSON BAD_REQUEST

let eventJObj = function
| TabOpened tab ->
    jobj [
        "event" .= "TabOpened"
        "data" .= tabJObj tab
    ]
| OrderPlaced order ->
    jobj [
        "event" .= "OrderPlaced"
        "data" .= jobj [
            "order" .= orderJObj order
        ]
    ]
| DrinkServed (drink,id) ->
    jobj [
        "event" .= "DrinkServed"
        "data" .= jobj [
            "drink" .= drinkJObj drink
            "tabId" .= id
        ]
    ]
| FoodPrepared (item,id) ->
    jobj [
        "event" .= "FoodPrepared"
        "data" .= jobj [
            "food" .= foodJObj item
            "tabId" .= id
        ]
    ]
| FoodServed (item, id) ->
    jobj [
        "event" .= "FoodServed"
        "data" .= jobj [
            "food" .= foodJObj item
            "tabId" .= id
        ]
    ]
| OrderServed (order,payment) ->
    jobj [
        "event" .= "OrderServed"
        "data" .= jobj [
            "order" .= orderJObj order
            "tabId" .= payment.Tab.Id
            "tableNumber" .= payment.Tab.TableNumber
            "amount" .= payment.Amount
        ]
    ]
| TabClosed payment ->
    jobj [
        "event" .= "TabClosed"
        "data" .= jobj [
            "amountPaid" .= payment.Amount
            "tabId" .= payment.Tab.Id
            "tableNumber" .= payment.Tab.TableNumber
        ]
    ]
| OrderModified order ->
    jobj [
        "event" .= "OrderPlaced"
        "data" .= jobj [
            "order" .= orderJObj order
        ]
    ]
