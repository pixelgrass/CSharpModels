using CSharpModels;
using System.Threading.Tasks;

namespace CSharpModelsTest.TrialModels
{
	public class ActorA : Actor
	{
		private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
		private int _count;
		/// <summary>
		/// This is a very bad implimentation and should not be used as an example - just an experiment
		/// </summary>
		/// <returns></returns>
		public Task<bool> TestMe()
		{
			_count++;
			if (_count == 100000)
			{
				_tcs.SetResult(true);
				return null;
			}

			Perform(() =>
			{
				TestMe();
			});

			return _tcs.Task;
		}
	}
}
