using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKG
{
	public class LogicPKG
	{
	}

	public class SafeAccessor // To prevent recursive calling between accessors
	{
		protected bool _isSettingValue; // backing field that keeps track of whether the setter is currently being executed.
		protected bool _value;

		public bool Value
		{
			get { return _value; }
			set
			{
				if (!_isSettingValue)
				{
					_isSettingValue = true;
					_value = value;
					// Perform any logic you need to do when the property is set
					_isSettingValue = false;
				}
				// Optionally, you can add an else block to handle what happens if the setter is already being executed.
				// This can be useful for handling potential recursive calls or raising an exception.
			}
		}
	}
	/// <summary>
	/// to update:
	/// Finite State Machine can has 4 types of actions:
	/// entry action
	/// exit action
	/// input action
	/// transition action
	/// </summary>
	public class FiniteStateMachine<S>
	{
		
		protected Dictionary<S, Action<object?>> stateActions = new Dictionary<S, Action<object?>>();
		protected S state;
		public object? eventData = null;

		public FiniteStateMachine(S defaultState)
		{
			// Initialize each action of states to an empty function of Action<T>
			foreach (S state in Enum.GetValues(typeof(S)))
			{
				stateActions[state] = (data) => { };
			}
			state = defaultState;
		}

		public S State
		{
			get { return state; }
			set
			{
				if (stateActions.ContainsKey(value))
				{
					state = value;
					TriggerEvent(state, eventData);
				}
			}
		}
		public void Subscribe_Actions(S state, Action<object?> action)
		{
			if (stateActions.ContainsKey(state))
			{
				stateActions[state] -= action;
				stateActions[state] += action;
			}
		}

		public void Unsubscribe_Actions(S state, Action<object?> action)
		{
			if (stateActions.ContainsKey(state))
			{
				stateActions[state] -= action;
			}
		}

		protected void TriggerEvent(S state, object? eventData)
		{
			if (stateActions.ContainsKey(state))
			{
				stateActions[state](eventData);
			}
		}
	}


}
