module Table

open Domain 
open System.Collections.Generic
open ReadModel
open Projections

let private tables =
    let dict = new Dictionary<int, Table>()
    dict.Add(1, {Number = 1; Waiter = "X"; Status = Closed})
    dict.Add(2, {Number = 2; Waiter = "Y"; Status = Closed})
    dict.Add(3, {Number = 3; Waiter = "Z"; Status = Closed})
    dict