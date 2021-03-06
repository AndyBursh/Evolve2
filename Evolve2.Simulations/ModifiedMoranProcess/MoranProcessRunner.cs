﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolve2.Simulations.ModifiedMoranProcess
{
    public class MoranProcessRunner
    {
        private IStateSelector _stateSelector;
        private IVertexSelector _vertexSelector;
        private IVictimSelector _victimSelector;
        private Random _random;

        public MoranProcessRunner(IStateSelector StateSelector, IVertexSelector VertexSelector, IVictimSelector VictimSelector) :
            this(StateSelector, VertexSelector, VictimSelector, Util.RandomProvider.Random)
        { }
        public MoranProcessRunner(IStateSelector StateSelector, IVertexSelector VertexSelector, IVictimSelector VictimSelector, Random Random)
        {
            _stateSelector = StateSelector;
            _vertexSelector = VertexSelector;
            _victimSelector = VictimSelector;
            _random = Random;
        }

        public MoranProcessResult RunOn<TIdent>(Graph<TIdent> G, int Repetitions, int Iterations, double MutantFitness)
            where TIdent : struct
        {
            MoranProcessResult result = new MoranProcessResult();
            result.RepetitionsPerformed = Repetitions;

            for (int rep = 0; rep < Repetitions; rep++)
            {
                Graph<TIdent> repGraph = (Graph<TIdent>)G.Clone();
                
                int iter = 0;
                while(iter < Iterations && !graphFixated(repGraph) && !graphExtinct(repGraph))
                {
                    IEnumerable<TIdent> targetState = _stateSelector.Select(repGraph, _random, MutantFitness);
                    TIdent vertex = _vertexSelector.Select(targetState, repGraph, _random);
                    IEnumerable<TIdent> destinationVertices = repGraph.VerticesConnectedToVertex(vertex); 
                    TIdent victim = _victimSelector.Select(destinationVertices, repGraph, _random);

                    //It's possible for there to be no victim.
                    //Consider the end of a chain where there are no neighbours
                    //No action should be taken, but it should still take up an iteration
                    if(!default(TIdent).Equals(victim))
                    {                  
                        StatefulVertex<TIdent, VertexState> vert = (StatefulVertex<TIdent, VertexState>)repGraph.FindVertex(vertex);
                        StatefulVertex<TIdent, VertexState> vict = (StatefulVertex<TIdent, VertexState>)repGraph.FindVertex(victim);

                        vict.State.ChangeStateValue(vert.State.CurrentState);
                    }
                    iter++;
                }

                if (!graphFixated(repGraph) && !graphExtinct(repGraph))
                {
                    result.Timeout++;
                }
                else
                {
                    if (graphFixated(repGraph) && !graphExtinct(repGraph))
                    {
                        result.Fixations++;
                    }
                    else if (!graphFixated(repGraph) && graphExtinct(repGraph))
                    {
                        result.Extinctions++;
                    }
                }
            }

            return result;
        }

        private bool graphFixated<T>(Graph<T> G) where T : struct
        {
            return G.Vertices.OfType<StatefulVertex<Guid, VertexState>>().All(v => v.State.CurrentState == VertexState.MUTANT);
        }

        private bool graphExtinct<T>(Graph<T> G) where T : struct
        {
            return G.Vertices.OfType<StatefulVertex<Guid, VertexState>>().All(v => v.State.CurrentState == VertexState.HEALTHY);
        }
    }
}
