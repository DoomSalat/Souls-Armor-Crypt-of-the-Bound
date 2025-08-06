public interface IAbilityArm : IAbility
{
	float SwordSpeed { get; }
	int SwordScaleIndex { get; }
	float SpeedThreshold { get; }

	public void SetSwordSettings(SwordController swordController);
}