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
		private int _messagesPerTick;
		private volatile int _tickFrequencyMilliseconds;
		private ITargetBlock<T> _target;
		private readonly Queue<T> _messageQueue;		

		private Task _messagePump;
		private bool _isActive;
		private bool _isCompleting;

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

		public override void Complete()
		{
			PerformLight(() =>
			{
				_isCompleting = true;
				if (!_isActive)
				{
					base.Complete();
				}
			});
		}

		/// <summary>
		/// start sending enqueued messages
		/// </summary>
		public void Start()
		{
			PerformLight(() =>
			{
				if (_isActive || _isCompleting)
				{
					return;
				}
				_isActive = true;
				_messagePump = MessageTimer();
			});
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
				if (_isCompleting)
				{
					base.Complete();
				}
			});
		}
		/// <summary>
		/// set how often to burst messages
		/// </summary>
		/// <param name="tickFrequencyMilliseconds">minimum time between bursts</param>
		public void SetFrequency(int tickFrequencyMilliseconds)
		{
			_tickFrequencyMilliseconds = tickFrequencyMilliseconds;
		}

		/// <summary>
		/// sets how many messages to send each burst
		/// </summary>
		/// <param name="messagesPerTick">number of messages to send each burst</param>
		public void SetMessagesPerTick(int messagesPerTick)
		{
			PerformLight(()=> { _messagesPerTick = messagesPerTick; });
		}

		/// <summary>
		/// change the message target
		/// </summary>
		/// <param name="target">where to send messages</param>
		public void SetTarget(ITargetBlock<T> target)
		{
			PerformLight(()=> { _target = target; });
		}

		private async Task MessageTimer()
		{
			while (_isActive && (Completion.Status != TaskStatus.RanToCompletion))
			{
				await SendMessages();
				var frequency = _tickFrequencyMilliseconds;
				if (frequency > 0)
				{
					await Task.Delay(frequency);
				}
			}
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
				if (_isCompleting && _messageQueue.Count == 0)
				{
					base.Complete();
				}			
			});
		}
	}
}
