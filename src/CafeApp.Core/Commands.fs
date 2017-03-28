module Commands

open Domain
open System

type Command =
| OpenTab of Tab
| PlaceOrder of Order
| ServeDrink of Drink * Guid
| PrepareFood of Food * Guid
| ServeFood of Food * Guid
| CloseTab of Payment
| ModifyOrder of Order

// todo the following
//Modifying an order after it has been placed, adding or removing item DONE
//Supporting multiple quantities of same item (like two cokes, three salads)
//Including tips amount in Payment ( just allow more? ) DONE
//Closing the Tab with partial order served
