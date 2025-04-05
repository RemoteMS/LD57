using System.Collections.Generic;

namespace Services.Gameplay.BulletSystem
{
    public interface IEffectDealer
    {
        public IEnumerable<Effect> Effects { get; }
    }
}