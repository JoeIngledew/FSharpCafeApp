module CafeAppTestsDSL

open FsUnit
open NUnit.Framework
open CommandHandlers
open States

let Given (state : State) = state
let When command state = (command, state)
let ThenStateShouldBe expectedState (command,state) =
  let actualState = evolve state command
  actualState |> should equal expectedState
let WithEvents expectedEvents actualEvents =
  actualEvents |> should equal expectedEvents
