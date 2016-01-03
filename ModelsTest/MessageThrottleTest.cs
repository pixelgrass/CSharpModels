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
			messageThrottle.Complete();
			messageThrottle.Completion.Wait(5000);
			Assert.AreEqual(6, count);
		}

		[TestMethod]
		public void ThrottleSpeedAndAmmountChanges()
		{
			var count = 0;
			var actionBlock = new ActionBlock<string>(s =>
			{
				count++;
			});
			var messageThrottle = new MessageThrottle<string>(actionBlock, 2, 1);
			messageThrottle.Start();
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test");
			}
			Task.Delay(50).Wait(5000);
			Assert.AreEqual(6, count);
			messageThrottle.SetFrequency(100);
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test");
			}
			Task.Delay(100).Wait(5000);			
			Assert.AreNotEqual(12, count);
			messageThrottle.SetMessagesPerTick(100);
			messageThrottle.SetFrequency(10);
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test");
			}
			
			messageThrottle.Complete();
			messageThrottle.Completion.Wait(5000);
			Assert.AreEqual(18,count);
		}


		[TestMethod]
		public void TargetSwapping()
		{
			var count = 0;
			var count2 = 0;
			var actionBlock = new ActionBlock<string>(s =>
			{
				count++;
			});
			var actionBlock2 = new ActionBlock<string>(s =>
			{
				count2++;
			});
			var messageThrottle = new MessageThrottle<string>(actionBlock, 2, 25);
			messageThrottle.Start();
			for (var i = 0; i < 10; i++)
			{
				messageThrottle.Post("test");
			}			
			Task.Delay(50).Wait(5000);
			messageThrottle.SetTarget(actionBlock2);
			Task.Delay(50).Wait(5000);
			messageThrottle.SetTarget(actionBlock2);
			messageThrottle.Complete();
			messageThrottle.Completion.Wait(5000);			
			Assert.AreEqual(10, count+count2);			
		}
	}
}
