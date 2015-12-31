using CSharpModels;
using System;
using System.Threading.Tasks;

namespace CSharpModelsTest.TrialModels
{
	class ErrorExampleActor : Actor
	{
		public Task ThrowAnError()
		{
			return Perform(() => { throw new InvalidProgramException("Something Went Wrong"); });
		}

		public Task<bool> MethodWithReturnThrowsAnError()
		{
			return Perform<bool>(() => { throw new InvalidProgramException("Something Went Wrong Part 2"); });
		}
	}
}
