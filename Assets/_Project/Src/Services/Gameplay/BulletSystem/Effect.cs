namespace Services.Gameplay.BulletSystem
{
    [System.Serializable]
    public abstract class Effect
    {
        public abstract void Apply(IEffectable target);
    }
}