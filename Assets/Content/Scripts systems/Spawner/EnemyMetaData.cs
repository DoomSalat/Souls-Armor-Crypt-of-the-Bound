namespace SpawnerSystem
{
	public class EnemyMetaData
	{
		public int TokensToReturn { get; private set; }
		public float TimerReductionOnDeath { get; private set; }
		public EnemyKind Kind { get; private set; }

		public EnemyMetaData(int tokensToReturn, float timerReductionOnDeath, EnemyKind kind = EnemyKind.Soul)
		{
			TokensToReturn = tokensToReturn;
			TimerReductionOnDeath = timerReductionOnDeath;
			Kind = kind;
		}

		public static EnemyMetaData FromGroupMeta(GroupMetaData groupMeta, EnemyKind kind = EnemyKind.Soul)
		{
			return new EnemyMetaData(groupMeta.TokensToReturn, groupMeta.TimerReductionOnDeath, kind);
		}
	}
}
