using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using CSharpModelsTest.TrialModels;
using System.Threading.Tasks;

namespace CSharpModelsTest
{
	[TestClass]
	public class ActorTest
	{
		[TestMethod]
		public void ActorOperationsInOrder()
		{
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
			Assert.AreEqual(100000, t.Result);
		}

		[TestMethod]
		public void ActorOperationsInOrderFast()
		{
			var actor = new ActorB();
			
			for (var j = 0; j < 100000; j++)
			{
				actor.AddFast(1);
				actor.VerifyFast(j);
			}
			
			var counter = actor.GetCounter();
			actor.Complete();
			var completionTask = actor.Completion;
			completionTask.Wait(5000);
			Assert.AreEqual(100000, counter.Result);
			AssertException(typeof(InvalidOperationException),null,() => actor.AddFast(1));			
		}

		/// <summary>
		/// no recommended to do this - just an experiment
		/// </summary>
		[TestMethod]
		public void ActorRecursiveCallbacks()
		{
			var actor = new ActorA();
			Assert.IsTrue(actor.TestMe().Result);
		}

		[TestMethod]
		public void CancellationTest()
		{
			var cancelationToken = new CancellationTokenSource();
			var actor = new ActorB(cancelationToken.Token);
			var tasks = new List<Task>(100000);
			for (var j = 0; j < 100000; j++)
			{
				tasks.Add(actor.Add(1));
				tasks.Add(actor.Verify(j));
			}
			cancelationToken.Cancel();
			AssertException(typeof(AggregateException),typeof(InvalidOperationException), () => actor.GetCounter());
			AssertException(typeof(AggregateException), typeof(TaskCanceledException), () => Task.WaitAll(tasks.ToArray()));
		}

		private static void AssertException(Type exceptionType, Type innerExceptionType, Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				Assert.AreEqual(exceptionType,e.GetType());
				if (null != innerExceptionType)
				{
					Assert.AreEqual(innerExceptionType, e.InnerException.GetType());
				}
			}
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
			AssertException(typeof(AggregateException),typeof(InvalidProgramException),() => task.Wait(5000));
		}

		[TestMethod]
		public void ErrorHandlingTestReturnValueMethod()
		{
			var errorActor = new ErrorExampleActor();
			var task = errorActor.MethodWithReturnThrowsAnError();
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
			Assert.AreEqual(TaskStatus.Faulted, task.Status);
		}
	}
}
