using System.Collections.Generic;
using System.Linq;

namespace Lockstep.Game 
{
    public interface ITimeMachineContainer : ITimeMachineService 
    {
        void RegisterTimeMachine(ITimeMachine roll);
    }

    public class TimeMachineContainer : ITimeMachineContainer 
    {
        public int CurTick { get; private set; }

        private HashSet<ITimeMachine> m_timeMachineHash = new HashSet<ITimeMachine>();
        private ITimeMachine[] m_allTimeMachines;

        private ITimeMachine[] GetAllTimeMachines()
        {
            if (m_allTimeMachines == null) 
            {
                m_allTimeMachines = m_timeMachineHash.ToArray();
            }

            return m_allTimeMachines;
        }

        public void RegisterTimeMachine(ITimeMachine roll)
        {
            if (roll != null && roll != this && m_timeMachineHash.Add(roll))
            {
                m_allTimeMachines = null;
            }
        }

        public void RollbackTo(int tick)
        {
            CurTick = tick;
            foreach (var timeMachine in GetAllTimeMachines()) 
            {
                timeMachine.RollbackTo(tick);
            }
        }

        public void Backup(int tick)
        {
            CurTick = tick;
            foreach (var timeMachine in GetAllTimeMachines()) 
            {
                timeMachine.Backup(tick);
            }
        }

        public void Clean(int maxVerifiedTick)
        {
            foreach (var timeMachine in GetAllTimeMachines()) 
            {
                timeMachine.Clean(maxVerifiedTick);
            }
        }
    }
}