using CSharpModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpModelsTest.TrialModels
{
	public class ActorB : Actor
	{
		public ActorB() {}

		public ActorB(CancellationToken cancellationToken) : base(cancellationToken){}
		private int _counter;
		public Task<int> Add(int a, int b)
		{
			return Perform(() => a + b);
		}

		public Task Add(int a)
		{
			return Perform(() =>
			{
				CancellationToken.ThrowIfCancellationRequested();
				_counter += a;
			});
		}

		public void AddFast(int a)
		{
			PerformLight(() =>
			{
				CancellationToken.ThrowIfCancellationRequested();
				_counter += a;
			});
		}

		private int _last = -1;
		public Task Verify(int j)
		{
			return Perform(() => {
				if (j <= _last)
				{
					throw new InvalidOperationException("Out of Sequence!");
				}
				_last = j;
			});
		}

		public void VerifyFast(int j)
		{
			PerformLight(() => {
				if (j <= _last)
				{
					throw new InvalidOperationException("Out of Sequence!");
				}
				_last = j;
			});
		}

		public Task<int> GetCounter()
		{
			return Perform(() => _counter);
		}

	}

}
