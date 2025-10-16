using SharpResults.Core;
using SharpResults.Types;

namespace SharpResults.Functional.State;

public class State<TState, T>
    where TState : notnull
    where T : notnull
{
    private readonly Func<TState, (T, TState)> _runState;

    public State(Func<TState, (T, TState)> runState)
    {
        _runState = runState;
    }

    public (T, TState) Run(TState state) => _runState(state);

    public State<TState, U> Select<U>(Func<T, U> f) where U : notnull
    {
        return new State<TState, U>(state =>
        {
            var (value, newState) = _runState(state);
            return (f(value), newState);
        });
    }

    public State<TState, U> SelectMany<U>(Func<T, State<TState, U>> f) where U : notnull
    {
        return new State<TState, U>(state =>
        {
            var (value, newState) = _runState(state);
            return f(value).Run(newState);
        });
    }

    public static State<TState, TState> Get() =>
        new(state => (state, state));

    public static State<TState, Unit> Put(TState newState) =>
        new(_ => (Unit.Default, newState));

    public static State<TState, Unit> Modify(Func<TState, TState> f) =>
        new(state => (Unit.Default, f(state)));
}