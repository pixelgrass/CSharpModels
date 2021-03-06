﻿using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CSharpModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpModelsTest
{
	[TestClass]
	public class MessageThrottleTest
	{
		/// <summary>
		/// because only 1 message will come out the comparitor processes the entire queue looking for the next unique message. (which it never finds)
		/// </summary>
		[TestMethod]
		public void ThrottleExcludeDuplicateMessagesTest()
		{
			var count = 0;
			var actionBlock = new ActionBlock<string>(s =>
			{
				count++;
			});
			var messageThrottle = new MessageThrottle<string>(actionBlock, 2, 100);
			messageThrottle.SetDuplicateMessageOptions(true, (s, s1) => s.Equals(s1));			
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test");
			}
			messageThrottle.Start();
			messageThrottle.Complete();
			messageThrottle.Completion.Wait(5000);
			actionBlock.Complete();
			actionBlock.Completion.Wait(5000);
			Assert.AreEqual(1, count);
		}

		/// <summary>
		/// Message burst size increased to 50 to ensure processing the entire queue and removing any duplicate messages over the range
		/// </summary>
		[TestMethod]
		public void ThrottleExcludeDuplicateMessagesTest2()
		{
			var count = 0;
			var actionBlock = new ActionBlock<string>(s =>
			{
				count++;
			});
			var messageThrottle = new MessageThrottle<string>(actionBlock, 50, 100);
			messageThrottle.SetDuplicateMessageOptions(true, (s, s1) => s.Equals(s1));
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test" + i);
			}
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test" + i);
			}
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test" + i);
			}
			messageThrottle.Start();
			messageThrottle.Complete();
			messageThrottle.Completion.Wait(5000);
			actionBlock.Complete();
			actionBlock.Completion.Wait(5000);
			Assert.AreEqual(6, count);
		}

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
			actionBlock.Complete();
			actionBlock.Completion.Wait(5000);
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
			messageThrottle.SetBurstSize(100);
			messageThrottle.SetFrequency(10);
			for (var i = 0; i < 6; i++)
			{
				messageThrottle.Post("test");
			}
			
			messageThrottle.Complete();
			messageThrottle.Completion.Wait(5000);
			actionBlock.Complete();
			actionBlock.Completion.Wait(5000);
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
			actionBlock.Complete();
			actionBlock.Completion.Wait(5000);
			Assert.AreEqual(10, count+count2);			
		}
	}
}
