namespace CustomPool
{
	public interface IPool
	{
		void ReturnToPool();
		void OnSpawnFromPool();
		void OnReturnToPool();
	}
}