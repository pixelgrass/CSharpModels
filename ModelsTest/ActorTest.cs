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
	}
}
