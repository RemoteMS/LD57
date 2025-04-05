namespace Services.Gameplay.BulletSystem
{
    public class DamageEffect : Effect
    {
        private int _damageValue = 19;

        public DamageEffect()
        {
        }

        public DamageEffect(int damageValue)
        {
            _damageValue = damageValue;
        }

        public override void Apply(IEffectable target)
        {
            target.Health.Damage(_damageValue);
        }
    }
}