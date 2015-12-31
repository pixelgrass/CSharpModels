using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CSharpModels
{
	public abstract class ActorZ
	{
		private readonly ActionBlock<Action> _actionBlock;
		public ActorZ()
		{
			_actionBlock = new ActionBlock<Action>(action => action());
		}

		protected Task<T> Schedule<T>(Func<T> func, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
		{
			var t = new Task<T>(() => { return func(); }, cancellationToken, creationOptions);
			_actionBlock.Post(() =>
			{
				t.Start(TaskScheduler.Default);
				t.Wait();
			});
			return t;
		}
		protected Task<T> Schedule<T>(Func<T> func)
		{
			return Schedule(func, CancellationToken.None, TaskCreationOptions.DenyChildAttach);
		}

		protected Task Schedule(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
		{
			var t = new Task(action, cancellationToken, creationOptions);
			_actionBlock.Post(() =>
			{
				t.Start(TaskScheduler.Default);
				t.Wait();
			});
			return t;
		}
		protected Task Schedule(Action action)
		{
			return Schedule(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach);
		}
	}
}
