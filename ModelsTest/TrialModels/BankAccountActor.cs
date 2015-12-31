using CSharpModels;
using System.Threading.Tasks;

namespace CSharpModelsTest.TrialModels
{
	class BankAccountActor : Actor
	{
		private decimal ballance;
		public Task Deposit (decimal ammount)
		{
			return Perform(() => { ballance += ammount; });
		}

		public Task<bool> Withdrawl (decimal ammount)
		{
			return Perform(() =>
			{
				if (ballance >= ammount)
				{
					ballance -= ammount;
					return true;
				}
				return false;
			});
		}

		public Task<decimal> GetBallance ()
		{
			return Perform(() => ballance);
		}
	}
}
