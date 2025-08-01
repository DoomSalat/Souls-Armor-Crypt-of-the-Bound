public interface IAbilityArm : IAbility
{
	float SwordSpeed { get; }
	int SwordScaleIndex { get; }

	public void SetSwordSettings(SwordController swordController);
}