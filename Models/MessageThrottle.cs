using System;
using System.Collections.Generic;
using System.Linq;
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
		private int _burstSize;
		private volatile int _tickFrequencyMilliseconds;
		private ITargetBlock<T> _target;
		private readonly Queue<T> _incomingQueue;
		private readonly List<T> _outgoingBuffer;
		private Func<T, T, bool> _compareFunc;
		private bool _isRemoveDuplicateMessagesEnabled;
		private Task _messagePump;
		private bool _isActive;
		private bool _isCompleting;

		public MessageThrottle(ITargetBlock<T> target, int burstSize, int tickFrequencyMilliseconds)
		{
			_target = target;
			_burstSize = burstSize;
			_tickFrequencyMilliseconds = tickFrequencyMilliseconds;
			_incomingQueue = new Queue<T>();
			_outgoingBuffer = new List<T>(_burstSize);
		}

		/// <summary>
		/// enqueue messages for sending
		/// </summary>
		/// <param name="message">the message to enqueue</param>
		public void Post(T message)
		{			
			PerformLight(() => _incomingQueue.Enqueue(message));
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
		/// <param name="burstSize">number of messages to send each burst</param>
		public void SetBurstSize(int burstSize)
		{
			PerformLight(() =>
			{
				_burstSize = burstSize;
				_outgoingBuffer.Capacity = _burstSize;
			});
		}

		/// <summary>
		/// when enabled processes the message queue for at least the burst size or at most thet entire queue in an attempt to send the burst size of messages.
		/// </summary>
		/// <param name="isRemoveDuplicateMessagesEnabled">dont sent messages identifed as duplicate by the comparison function (within the burst range).</param>
		/// <param name="comparisonFunction">function used to compare messages</param>
		public void SetDuplicateMessageOptions(bool isRemoveDuplicateMessagesEnabled, Func<T, T, bool> comparisonFunction )
		{
			PerformLight(() =>
			{
				_isRemoveDuplicateMessagesEnabled = isRemoveDuplicateMessagesEnabled;
				_compareFunc = comparisonFunction;
			});
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
					while ((_outgoingBuffer.Count < _burstSize) && (_incomingQueue.Count > 0))
					{
						var message = _incomingQueue.Dequeue();
						if (!_isRemoveDuplicateMessagesEnabled || 
						(_isRemoveDuplicateMessagesEnabled &&
						!_outgoingBuffer.Any(m => _compareFunc(m,message))))
						{
							_outgoingBuffer.Add(message);
						}						
					}
					var bufferSize = _outgoingBuffer.Count;
					for (var i = 0; i < bufferSize; i++)
					{
						_target.Post(_outgoingBuffer[i]);
					}
					_outgoingBuffer.Clear();
				}
				if (_isCompleting && _incomingQueue.Count == 0)
				{
					base.Complete();
				}			
			});
		}
	}
}
