using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CSharpModels
{
	public abstract class Actor
	{
		private readonly ActionBlock<Action> _actionBlock;
		public Actor()
		{
			_actionBlock = new ActionBlock<Action>(action => action());
		}
		protected Task<T> Schedule<T>(Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>();			
			_actionBlock.Post(()=>tcs.SetResult(func()));
			return tcs.Task;
		}
		protected Task Schedule(Action action)
		{
			var tcs = new TaskCompletionSource<object>();
			_actionBlock.Post(() =>
			{
				action();
				tcs.SetResult(null);
			});
			return tcs.Task;
		}
	}
}
