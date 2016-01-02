using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CSharpModels
{
	/// <summary>
	/// To be used when sending bursts of messages at a specified frequency is desired
	/// Thread Safe - using the actor model
	/// </summary>
	/// <typeparam name="T">The message type to enqueue</typeparam>
	public class MessageThrottle<T> : Actor
	{
		private readonly int _messagesPerTick;
		private readonly int _tickFrequencyMilliseconds;
		private readonly ITargetBlock<T> _target;
		private readonly Queue<T> _messageQueue;		

		private Task _messagePump;
		private bool _isActive;

		public MessageThrottle(ITargetBlock<T> target, int messagePerTick, int tickFrequencyMilliseconds)
		{
			_target = target;
			_messagesPerTick = messagePerTick;
			_tickFrequencyMilliseconds = tickFrequencyMilliseconds;
			_messageQueue = new Queue<T>();
		}

		/// <summary>
		/// enqueue messages for sending
		/// </summary>
		/// <param name="message">the message to enqueue</param>
		public void Post(T message)
		{
			PerformLight(() => _messageQueue.Enqueue(message));
		}

		/// <summary>
		/// start sending enqueued messages
		/// </summary>
		public void Start()
		{
			PerformLight(() =>
			{
				if (!_isActive)
				{
					_isActive = true;
					_messagePump = MessageTimer();
				}
			});
		}

		private async Task MessageTimer()
		{
			while (_isActive)
			{
				await SendMessages();
				var frequency = _tickFrequencyMilliseconds;
				if (frequency > 0)
				{
					await Task.Delay(_tickFrequencyMilliseconds);
				}
			}
		}

		/// <summary>
		/// Stop sending messages - existing messagese will still be queued . new messages may still be posted.
		/// </summary>
		public void Stop()
		{
			PerformLight(() =>
			{
				_isActive = false;
				_messagePump.Wait();
				_messagePump = null;
			});
		}

		private Task SendMessages()
		{
			return Perform(() =>
			{
				if (!_isActive)
				{
					return;
				}
				if (null != _target)
				{
					for (var i = 0; (i < _messagesPerTick) && (_messageQueue.Count > 0); i++)
					{
						_target.Post(_messageQueue.Dequeue());
					}
				}				
			});
		}
	}
}
