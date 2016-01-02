using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CSharpModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpModelsTest
{
	[TestClass]
	public class MessageThrottleTest
	{
		[TestMethod]
		public void ThrottleTest()
		{
			var count = 0;
			var actionBlock = new ActionBlock<string>(s =>
			{
				count ++;
			});
			var messageThrottle = new MessageThrottle<string>(actionBlock,2,100);
			messageThrottle.Start();
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test");
			}
			Task.Delay(50).Wait(5000);
			Assert.AreNotEqual(4,count);
			Task.Delay(100).Wait(5000);
			Assert.IsTrue(count > 1);
			Assert.AreNotEqual(6, count);
			messageThrottle.Stop();
			Task.Delay(200).Wait(5000);
			Assert.IsTrue(count > 1);
			Assert.AreNotEqual(6, count);
			messageThrottle.Start();
			Task.Delay(200).Wait(5000);
			Assert.AreEqual(6, count);
		}
	}
}
