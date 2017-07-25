using System.ComponentModel;

namespace viz
{
	public interface IStatePainter<in TState> : IScenePainter
	{
		void Update(TState state);
	}

	public interface IChangeLog
	{
		bool IsUpdating { get; }
		int Length { get; }
	}

	public interface IChangeLog<out TState> : IChangeLog
	{
		TState this[int index] { get; }
	}
}