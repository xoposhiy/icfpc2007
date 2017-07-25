using System;

namespace viz
{
	public class PlaybackController
	{
		private readonly IChangeLog changeLog;
		private int interval;
		private int stepSize;
		private int time;

		public PlaybackController(IChangeLog changeLog)
		{
			this.changeLog = changeLog;
		}

		public bool WaitingForData { get; private set; }
		public int Position { get; set; }
		public bool IsPlaying { get; private set; }

		private void DoStep()
		{
			var oldPosition = Position;
			Position = Math.Min(changeLog.Length - 1, Math.Max(0, Position + stepSize));
			if (Position == 0 || Position == changeLog.Length - 1 && !changeLog.IsUpdating)
				Stop();
			WaitingForData = Position == changeLog.Length - 1 && changeLog.IsUpdating;
			if (Position != oldPosition)
				PositionChanged?.Invoke(this);
		}

		public void Stop()
		{
			IsPlaying = false;
		}

		public void On50TimesPerSecond()
		{
			if (!IsPlaying) return;
			time += 50;
			while (time >= interval)
			{
				DoStep();
				time -= interval;
			}
		}

		public void Play(double fps, int aStepSize)
		{
			stepSize = aStepSize;
			interval = (int) Math.Round(1000 / fps);
			IsPlaying = true;
		}

		public void Step(int aStepSize)
		{
			Stop();
			stepSize = aStepSize;
			DoStep();
		}

		public event Action<PlaybackController> PositionChanged;
	}
}