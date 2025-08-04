public interface IAbilityBody : IAbility
{
	bool CanBlockDamage();
	void DamageBlocked();
}