module QueriesApi

open Queries
open Suave
open Suave.Filters
open Suave.Operators
open JsonFormatter
open CommandHandlers

let readModels getReadModels wp (context : HttpContext) =
    async {
        let! models = getReadModels ()
        return! wp models context
    }

let queriesApi queries eventStore =
    GET >=>
        choose [
            path "/tables" >=>
                readModels queries.Table.GetTables toTablesJson
            path "/todos/chef" >=>
                readModels queries.ToDo.GetChefToDos toChefToDosJson
            path "/todos/waiter" >=>
                readModels queries.ToDo.GetWaiterToDos toWaiterToDosJson
            path "/todos/cashier" >=>
                readModels queries.ToDo.GetCashierToDos toCashierToDosJson
            path "/foods" >=>
                readModels queries.Food.GetFoods toFoodsJson
            path "/drinks" >=> 
                readModels queries.Drink.GetDrinks toDrinksJson
        ]