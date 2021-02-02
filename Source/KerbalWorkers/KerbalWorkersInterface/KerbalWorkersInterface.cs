namespace KerbalWorkersInterface
{
    public interface IKerbalWorkersCallsReciever
    {
        void ForcedDeoccupy(string kerbalName);
    }

    public abstract class KerbalWorkersAbstractScenario : ScenarioModule
    {
        public abstract void RegisterCallsReciever(IKerbalWorkersCallsReciever kerbalWorkersCallsReciever, string associatedName);

        public abstract bool Occupy(string kerbalName, string associatedCallsRecieverName, bool allowForcedReoccupation);

        public abstract bool ForcedOccupyKerbal(string kerbalName, string associatedCallsRecieverName, bool allowForcedReoccupation);

        public abstract void Deoccupy(string kerbalName, string associatedCallsRecieverName);
    }
}
