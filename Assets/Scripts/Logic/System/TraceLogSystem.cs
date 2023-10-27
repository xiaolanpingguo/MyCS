using System.Text;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Math;

namespace Lockstep.Game 
{
    public class TraceLogSystem : BaseSystem 
    {
        StringBuilder m_dumpSb = new StringBuilder();

        public override void DoUpdate(LFloat deltaTime)
        {
            m_dumpSb.AppendLine("Tick: " + World.Instance.Tick);

            //trace input
            foreach (var input in World.Instance.PlayerInputs)
            {
                DumpInput(input);
            }

            foreach (var entity in _gameStateService.GetPlayers())
            {
                DumpEntity(entity);
            }

            foreach (var entity in _gameStateService.GetEnemies())
            {
                //dumpSb.Append(" " + entity.timer);
                DumpEntity(entity);
            }

            //_debugService.Trace(m_dumpSb.ToString(), true);
            m_dumpSb.Clear();
        }

        private void DumpInput(PlayerInput input)
        {
            m_dumpSb.Append("    ");
            m_dumpSb.Append(" skillId:" + input.skillId);
            m_dumpSb.Append(" " + input.mousePos);
            m_dumpSb.Append(" " + input.inputUV);
            m_dumpSb.Append(" " + input.isInputFire);
            m_dumpSb.Append(" " + input.isSpeedUp);
            m_dumpSb.AppendLine();
        }


        private void DumpEntity(BaseEntity entity)
        {
            m_dumpSb.Append("    ");
            m_dumpSb.Append(" " + entity.EntityId);
            m_dumpSb.Append(" " + entity.transform.Pos3);
            m_dumpSb.Append(" " + entity.transform.deg);
            m_dumpSb.AppendLine();
        }
    }
}