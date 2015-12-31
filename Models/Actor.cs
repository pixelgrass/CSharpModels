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
		protected Task<T> Perform<T>(Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>();			
			_actionBlock.Post(()=>
			{
				try
				{
					tcs.SetResult(func());
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			});
			return tcs.Task;
		}
		protected Task Perform(Action action)
		{
			var tcs = new TaskCompletionSource<object>();
			_actionBlock.Post(() =>
			{
				try
				{
					action();
					tcs.SetResult(null);
				}
				catch(Exception e)
				{
					tcs.SetException(e);
				}				
			});
			return tcs.Task;
		}
	}
}
