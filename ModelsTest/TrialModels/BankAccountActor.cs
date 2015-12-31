using CSharpModels;
using System;
using System.Threading.Tasks;

namespace CSharpModelsTest.TrialModels
{
	class BankAccountActor : Actor
	{
		private decimal ballance;

		#region PublicAsyncApi
		public Task Deposit (decimal ammount)
		{
			return Perform(() => { ballance += ammount; });
		}

		public Task<bool> Withdrawl (decimal ammount)
		{
			return Perform(()=>LocalWithdrawl(ammount));
		}
		
		public Task<decimal> GetBallance ()
		{
			return Perform(() => ballance);
		}

		public Task<bool> Transfer(decimal ammount, BankAccountActor toAccount)
		{
			return Perform(() => LocalTransfer(ammount,toAccount));
		}
		#endregion
		#region PrivateSynchronous
		private bool LocalWithdrawl(decimal ammount)
		{
			if (ballance >= ammount)
			{
				ballance -= ammount;
				return true;
			}
			return false;
		}
		private bool LocalTransfer(decimal ammount, BankAccountActor toAccount)
		{
			if (LocalWithdrawl(ammount))
			{
				toAccount.Deposit(ammount);
				return true;
			}
			return false;
		}
		#endregion
	}
}
