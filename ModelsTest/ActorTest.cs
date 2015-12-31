using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CSharpModelsTest.TrialModels;
using System.Threading.Tasks;

namespace CSharpModelsTest
{
	[TestClass]
	public class ActorTest
	{
		[TestMethod]
		public void ActorSequencing()
		{
			var counter = 0;
			var actor = new ActorB();
			var tasks = new List<Task>(100000);
			for (var j = 0; j < 100000; j++)
			{
				tasks.Add(actor.Add(1));
				tasks.Add(actor.Verify(j));
			}
			Task.WaitAll(tasks.ToArray());
			var t = actor.GetCounter();
			t.Wait(5000);
			counter = t.Result;
			Assert.AreEqual(100000, counter);
		}

		[TestMethod]
		public void ActorRecursiveCallbacks()
		{
			var actor = new ActorA();
			var t = actor.TestMe();
			t.Wait(5000);
			Assert.IsTrue(t.Result);
		}

		[TestMethod]
		public void BankAccountTest()
		{
			var account = new BankAccountActor();
			account.Deposit(10);
			Assert.IsFalse(account.Withdrawl(20).Result);
			Assert.AreEqual(10, account.GetBallance().Result);
			account.Deposit(10);
			account.Deposit(10);
			account.Deposit(10);
			Assert.IsTrue(account.Withdrawl(40).Result);
			Assert.AreEqual(0, account.GetBallance().Result);
		}

		[TestMethod]
		public void BankAccountTransferTest()
		{
			var accountA = new BankAccountActor();
			var accountB = new BankAccountActor();
			accountA.Deposit(100);
			accountA.Transfer(25, accountB);
			Assert.AreEqual(75, accountA.GetBallance().Result);
			Assert.AreEqual(25, accountB.GetBallance().Result);
		}

		[TestMethod]
		public void ErrorHandlingTest()
		{
			var errorActor = new ErrorExampleActor();
			var task = errorActor.ThrowAnError();
			AggregateException ex = null;
			try
			{
				task.Wait(5000);
			}
			catch (AggregateException aggEx)
			{
				ex = aggEx;
			}
			Assert.IsNotNull(ex);
			Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidProgramException));
		}

		[TestMethod]
		public void ErrorHandlingTestReturnValueMethod()
		{
			var errorActor = new ErrorExampleActor();
			var task = errorActor.MethodWithReturnThrowsAnError();
			AggregateException ex = null;
			try
			{
				task.Result.ToString();
			}
			catch (AggregateException aggEx)
			{
				ex = aggEx;
			}
			Assert.IsNotNull(ex);
			Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidProgramException));
			Assert.AreEqual(TaskStatus.Faulted, task.Status);
		}
	}
}
