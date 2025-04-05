using System.Collections.Generic;

namespace Services.Gameplay.BulletSystem
{
    public interface IEffectDealer
    {
        public List<Effect> Effects { get; }
    }
}