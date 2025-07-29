namespace StatusSystem
{
	public interface IStatus
	{
		void Initialize(IDamageable damageable);
		void OnStatusEnded();
	}
}