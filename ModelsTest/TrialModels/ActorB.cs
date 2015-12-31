using CSharpModels;
using System;
using System.Threading.Tasks;

namespace CSharpModelsTest.TrialModels
{
	public class ActorB : Actor
	{
		private int counter = 0;
		public Task<int> Add(int a, int b)
		{
			return Perform(() =>
			{
				return a + b;
			});
		}

		public Task Add(int a)
		{
			return Perform(() => { counter += a; });
		}
		int last = -1;
		public Task Verify(int j)
		{
			return Perform(() => {
				if (j <= last)
				{
					throw new InvalidOperationException("Out of Sequence!");
				}
				last = j;
			});
		}

		public Task<int> GetCounter()
		{
			return Perform(() =>
			{
				return counter;
			});
		}

		public Task DoNothing()
		{
			return Perform(() => { });
		}
	}

}
