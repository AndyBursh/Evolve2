﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolve2.State
{
    public abstract class StateBase<T> : IState<T>
    {
        public T CurrentState { get; set; }

        public StateBase(T InitialValue)
        {
            CurrentState = InitialValue;
        }

        public abstract int CompareTo(IState<T> other);
        public abstract bool ChangeStateValue(T NewStateValue);
        public abstract bool StateChangeIsValid(T NewStateValue);
    }
}
