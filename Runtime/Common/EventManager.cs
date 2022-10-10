using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [ExecuteAlways]
    public static class EventManager {
        private static Dictionary<Type, List<IEventListenerBase>> _subscribersList;

        static EventManager() {
            _subscribersList = new Dictionary<Type, List<IEventListenerBase>>();
        }

        /// <summary>
        /// Adds a new subscriber to a certain event.
        /// </summary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        public static void AddListener<TEvent>(IEventListener<TEvent> listener) where TEvent : struct {
            Type eventType = typeof(TEvent);

            if (!_subscribersList.ContainsKey(eventType)) {
                _subscribersList[eventType] = new List<IEventListenerBase>();
            }

            if (!SubscriptionExists(eventType, listener)) {
                _subscribersList[eventType].Add(listener);
            }
        }

        /// <summary>
        /// Removes a subscriber from a certain event.
        /// </summary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        public static void RemoveListener<TEvent>(IEventListener<TEvent> listener) where TEvent : struct {
            Type eventType = typeof(TEvent);

            if (!_subscribersList.ContainsKey(eventType)) {
                #if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
                #else
                return;
                #endif
            }

            List<IEventListenerBase> subscriberList = _subscribersList[eventType];

            #if EVENTROUTER_THROWEXCEPTIONS
	            bool listenerFound = false;
            #endif

            for (int i = subscriberList.Count - 1; i >= 0; i--) {
                if (subscriberList[i] == listener) {
                    subscriberList.Remove(subscriberList[i]);
                    #if EVENTROUTER_THROWEXCEPTIONS
					    listenerFound = true;
                    #endif

                    if (subscriberList.Count == 0) {
                        _subscribersList.Remove(eventType);
                    }

                    return;
                }
            }

            #if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
            #endif
        }

        /// <summary>
        /// Triggers an event. All instances that are subscribed to it will receive it (and will potentially act on it).
        /// </summary>
        /// <param name="newEvent">The event to trigger.</param>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        public static void TriggerEvent<TEvent>(TEvent newEvent) where TEvent : struct {
            List<IEventListenerBase> list;
            if (!_subscribersList.TryGetValue(typeof(TEvent), out list))
                #if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
                #else
                return;
            #endif

            for (int i = list.Count - 1; i >= 0; i--) {
                (list[i] as IEventListener<TEvent>)?.OnMMEvent(newEvent);
            }
        }

        /// <summary>
        /// Checks if there are subscribers for a certain type of events
        /// </summary>
        /// <returns><c>true</c>, if exists was subscriptioned, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        /// <param name="receiver">Receiver.</param>
        private static bool SubscriptionExists(Type type, IEventListenerBase receiver) {
            List<IEventListenerBase> receivers;

            if (!_subscribersList.TryGetValue(type, out receivers)) return false;

            bool exists = false;

            for (int i = receivers.Count - 1; i >= 0; i--) {
                if (receivers[i] == receiver) {
                    exists = true;
                    break;
                }
            }

            return exists;
        }
    }

    /// <summary>
    /// Static class that allows any class to start or stop listening to events
    /// </summary>
    public static class EventRegister {
        public delegate void Delegate<in T>(T type);

        public static void EventStartListening<TEventType>(this IEventListener<TEventType> caller) where TEventType : struct {
            EventManager.AddListener<TEventType>(caller);
        }

        public static void EventStopListening<TEventType>(this IEventListener<TEventType> caller) where TEventType : struct {
            EventManager.RemoveListener<TEventType>(caller);
        }
    }

    /// <summary>
    /// Event listener basic interface
    /// </summary>
    public interface IEventListenerBase { };

    /// <summary>
    /// A public interface you'll need to implement for each type of event you want to listen to.
    /// </summary>
    public interface IEventListener<in T> : IEventListenerBase {
        void OnMMEvent(T eventType);
    }

}