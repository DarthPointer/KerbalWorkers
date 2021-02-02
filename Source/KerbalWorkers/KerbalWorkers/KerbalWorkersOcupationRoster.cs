using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalWorkersInterface;

namespace KerbalWorkers
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new[] { GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.LOADING, GameScenes.LOADINGBUFFER })]
    public class KerbalWorkersOccupatuionRoster : KerbalWorkersAbstractScenario
    {
        private Dictionary<string, CurrentOccupationData> occupiedKerbals = new Dictionary<string, CurrentOccupationData>();
        private Dictionary<string, IKerbalWorkersCallsReciever> registeredCallsRecievers = new Dictionary<string, IKerbalWorkersCallsReciever>();

        public override void OnAwake()
        {
            base.OnAwake();
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }

        public override void RegisterCallsReciever(IKerbalWorkersCallsReciever kerbalWorkersCallsReciever, string associatedName)
        {
            registeredCallsRecievers[associatedName] = kerbalWorkersCallsReciever;
        }

        public override bool Occupy(string kerbalName, string associatedCallsRecieverName, bool allowForcedReoccupation)   
        {
            if (!registeredCallsRecievers.ContainsKey(associatedCallsRecieverName))
            {
                Debug.LogError($"[KerbalWorkers] Not going to register occupation with unregistered associated calls reciever name {associatedCallsRecieverName}");
                return false;
            }

            if (occupiedKerbals.ContainsKey(kerbalName))
            {
                return false;
            }
            else
            {
                HighLogic.CurrentGame.CrewRoster[kerbalName].trait += "Occupied";

                occupiedKerbals.Add(kerbalName, new CurrentOccupationData()
                {
                    allowsForcedReoccupation = allowForcedReoccupation,
                    associatedCallsRecieverName = associatedCallsRecieverName
                });
                return true;
            }
        }

        public override bool ForcedOccupyKerbal(string kerbalName, string associatedCallsRecieverName, bool allowForcedReoccupation) // Returns true if succeeded, tries to reoccupy currently occupied kerbals
        {
            if (ForcedReoccupation(kerbalName))
            {
                HighLogic.CurrentGame.CrewRoster[kerbalName].trait.Replace("Occupied", "");
                return Occupy(kerbalName, associatedCallsRecieverName, allowForcedReoccupation);
            }

            return false;
        }

        private bool ForcedReoccupation(string kerbalName)      // Performs forced reoccupation. Returns false if failed (kerbal occupied and FR not allowed), true if succeeded (kerbal not occupied or was occupied but deoccupaition was allowed and succeeded)
        {
            if (!occupiedKerbals.ContainsKey(kerbalName))
            {
                return true;
            }

            if (occupiedKerbals[kerbalName].allowsForcedReoccupation)
            {
                registeredCallsRecievers[occupiedKerbals[kerbalName].associatedCallsRecieverName].ForcedDeoccupy(kerbalName);
                return true;
            }

            return false;
        }

        public override void Deoccupy(string kerbalName, string associatedCallsRecieverName)
        {
            if (!occupiedKerbals.ContainsKey(kerbalName))
            {
                Debug.LogError($"[KerbalWorkers] Kerbal deoccupation is called for {kerbalName} but it is not occupied");
                return;
            }

            if (occupiedKerbals[kerbalName].associatedCallsRecieverName != associatedCallsRecieverName)
            {
                Debug.LogError($"[KerbalWorkers] {associatedCallsRecieverName} requested deoccupation for Kerbal {kerbalName} " +
                    $"who is marked as occupied by {occupiedKerbals[kerbalName].associatedCallsRecieverName}");
            }
        }

        public void OnDestroy()
        {
        }

        private struct CurrentOccupationData
        {
            public string associatedCallsRecieverName;
            public bool allowsForcedReoccupation;
        }
    }
}
