using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractionDirecting.Application
{

	public interface IState<TStateEnum> where TStateEnum : Enum
	{
		TStateEnum StateEnum { get; }
		void EntryAction();
		void ExitAction();
	}

	public interface IInput<TTrigger>
	{
		TTrigger Trigger { get; }
		// Any additional properties or methods for the input can be defined here
	}

	public interface ITransition<TStateEnum, TTrigger, TInput>
		where TStateEnum : Enum
		where TInput : IInput<TTrigger>
	{
		IState<TStateEnum> SourceState { get; }
		IState<TStateEnum> TargetState { get; }
		TTrigger Trigger { get; }
		void TransitionAction(TInput input);
	}

	public class StateMachine<TStateEnum, TTrigger, TInput>
		where TStateEnum : Enum
		where TInput : IInput<TTrigger>
	{
		private readonly Dictionary<(TStateEnum, TTrigger), ITransition<TStateEnum, TTrigger, TInput>> 
			_transitions = new Dictionary<(TStateEnum, TTrigger), ITransition<TStateEnum, TTrigger, TInput>>();
		private IState<TStateEnum> _currentState;

		public StateMachine(IState<TStateEnum> initialState)
		{
			_currentState = initialState;
			_currentState.EntryAction();
		}

		public TStateEnum CurrentStateEnum => _currentState.StateEnum;

		public void AddTransition(ITransition<TStateEnum, TTrigger, TInput> transition)
		{
			_transitions[(transition.SourceState.StateEnum, transition.Trigger)] = transition;
		}

		public void HandleInput(TInput input)
		{
			if (_transitions.TryGetValue((_currentState.StateEnum, input.Trigger), out var transition))
			{
				_currentState.ExitAction();
				transition.TransitionAction(input);
				_currentState = transition.TargetState;
				_currentState.EntryAction();
			}
			else
			{
				throw new InvalidOperationException($"No transition from state {_currentState.StateEnum} for trigger {input.Trigger}");
			}
		}
	}
}
