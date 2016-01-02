using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CSharpModels
{
	/// <summary>
	/// The Actor model is a threading model used for concurrent computation.
	/// </summary>
	/// <remarks>
	/// All logic inside perform requests need to be synchronous (dont use await / async)
	/// It is recommended to not create any worker threads inside perform requests as this deviates from the models intention
	/// </remarks>
	/// <seealso cref="http://en.wikipedia.org/wiki/Actor_model"/>
	public abstract class Actor
	{
		private readonly ActionBlock<Action> _actionBlock;
		protected CancellationToken CancellationToken { get; }

		protected Actor()
		{			
			_actionBlock = new ActionBlock<Action>(action => action(), new ExecutionDataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded,MaxDegreeOfParallelism = 1});			
		}

		protected Actor(CancellationToken cancellationToken) :this()
		{
			CancellationToken = cancellationToken;
			cancellationToken.Register(() =>
			{
				_actionBlock.Complete();
			});
		}

		/// <summary>
		/// Signal the Actor to complete all performances and stop accepting new requests
		/// </summary>
		public void Complete()
		{
			_actionBlock.Complete();
		}

		/// <summary>
		/// Gets a Task object that represents the async operation of the Actor and completion of the perform requests
		/// </summary>
		public Task Completion => _actionBlock.Completion;

		/// <summary>
		/// Perform a function that returns a value of a specified type
		/// </summary>
		/// <typeparam name="T">The type that will be returned from the function</typeparam>
		/// <param name="func">The function that this actor should perform</param>
		/// <returns>A task that can be used to retrieve the return value, determin completion and any exceptions </returns>
		protected Task<T> Perform<T>(Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>();
			if (!_actionBlock.Post(() =>
			{
				if (CancellationToken.IsCancellationRequested)
				{
					tcs.SetCanceled();
					return;
				}
				try
				{
					tcs.SetResult(func());
				}
				catch (OperationCanceledException)
				{
					tcs.SetCanceled();
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}))
			{
				tcs.SetException(new InvalidOperationException("Actor is unable to perform the requested action in its current state."));
			}
			return tcs.Task;
		}
		/// <summary>
		/// Perform an action that does not return a value
		/// </summary>
		/// <param name="action">The action this actor should perform</param>
		/// <returns>A task that can be used to determin completion and any exceptions </returns>
		protected Task Perform(Action action)
		{
			return Perform<object>(() => 
			{
				action();
				return null;
			});			
		}

		/// <summary>
		/// Perform an action that does not return a value. Recommended when higher throughput is required.
		/// Does not create a task to track the action completion.
		/// </summary>
		/// <remarks>
		/// There is no error handeling around these actions. Throwing exceptions in these actions will cause the actor to stop responding.
		/// </remarks>
		/// <param name="action">the action to perform</param>
		protected void PerformLight(Action action)
		{
			if (!_actionBlock.Post(() =>
			{
				if (CancellationToken.IsCancellationRequested) { return;}
				try { action();}
				catch (OperationCanceledException){}				
			}))
			{
				throw new InvalidOperationException("Actor is unable to perform the requested action in its current state.");
			}			
		}
	}
}
