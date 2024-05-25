([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Computable.Automata)](https://www.nuget.org/packages/LfrlAnvil.Computable.Automata/)

# [LfrlAnvil.Computable.Automata](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Computable/LfrlAnvil.Computable.Automata)

This project contains structures and algorithms related to the automata theory.

### Examples

Following is an example of a deterministic finite state machine (FSM):
```csharp
public enum TurnstileState
{
    Locked = 0,
    Unlocked = 1
}

public enum TurnstileInput
{
    Coin = 0,
    Push = 1
}

public enum TurnstileOutput
{
    Unchanged = 0,
    Locked = 1,
    Unlocked = 2
}

// creates a new state machine builder that will represent a turnstile state machine
var builder = new StateMachineBuilder<TurnstileState, TurnstileInput, TurnstileOutput>( TurnstileOutput.Unchanged );

// registers a 'Locked' => 'Locked' transition with 'Push' input, that returns the default 'Unchanged' output
builder.AddTransition(
    source: TurnstileState.Locked,
    destination: TurnstileState.Locked,
    input: TurnstileInput.Push );

// registers a 'Locked' => 'Unlocked' transition with 'Coin' input that returns the 'Unlocked' output
builder.AddTransition(
    source: TurnstileState.Locked,
    destination: TurnstileState.Unlocked,
    input: TurnstileInput.Coin,
    handler: StateTransitionHandler.Create<TurnstileState, TurnstileInput, TurnstileOutput>( _ => TurnstileOutput.Unlocked ) );

// registers an 'Unlocked' => 'Unlocked' transition with 'Coin' input, that returns the default 'Unchanged' output
builder.AddTransition(
    source: TurnstileState.Unlocked,
    destination: TurnstileState.Unlocked,
    input: TurnstileInput.Coin );

// registers an 'Unlocked' => 'Locked' transition with 'Coin' input that returns the 'Locked' output
builder.AddTransition(
    source: TurnstileState.Unlocked,
    destination: TurnstileState.Locked,
    input: TurnstileInput.Push,
    handler: StateTransitionHandler.Create<TurnstileState, TurnstileInput, TurnstileOutput>( _ => TurnstileOutput.Locked ) );

// marks the 'Locked' state as initial
builder.MarkAsInitial( TurnstileState.Locked );

// builds the state machine
var machine = builder.Build();

// creates a traversable instance of the state machine that starts at the initial 'Locked' state
var instance = machine.CreateInstance();

// applies 'Push' input to the current 'Locked' state,
// which does not change the state and returns 'Unchanged' output
var result = instance.Transition( TurnstileInput.Push );

// applies 'Coin' input to the current 'Locked' state,
// which changes the state to 'Unlocked' and returns 'Unlocked' output
result = instance.Transition( TurnstileInput.Coin );

// applies 'Coin' input to the current 'Unlocked' state,
// which does not change the state and returns 'Unchanged' output
result = instance.Transition( TurnstileInput.Coin );

// applies 'Push' input to the current 'Unlocked' state,
// which changes the state to 'Locked' and returns 'Locked' output
result = instance.Transition( TurnstileInput.Push );
```
