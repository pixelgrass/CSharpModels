using CSharpModels;
using System.Threading.Tasks;

namespace CSharpModelsTest.TrialModels
{
	public class ActorA : Actor
	{
		TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
		int count = 0;
		public Task<bool> TestMe()
		{
			count++;
			if (count == 100000)
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
