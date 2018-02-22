using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeGenie.Enums;
using Stateless;

namespace HomeGenie.Data
{
    [Serializable]
    public class Location
    {
        /// <summary>
        /// The occupancy timer
        /// </summary>
        private readonly System.Timers.Timer occupancyTimer;

        /// <summary>
        /// The state machine
        /// </summary>
        private readonly StateMachine<OccupancyState, Trigger> _stateMachine;

        /// <summary>
        /// Prevents a default instance of the <see cref="Location"/> class from being created.
        /// </summary>
        private Location()
        {
            // parameterless constructor for serialization
        }

        /// <summary>
        /// Creates the state machine.
        /// </summary>
        /// <returns></returns>
        // TODO look how to inject this from the program helper bits.
        private StateMachine<OccupancyState, Trigger> CreateStateMachine()
        {
            var stateMachine = new StateMachine<OccupancyState, Trigger>(OccupancyState.UnOccupied);

            // stateMachine.OnTransitioned(OnTransitionedAction);

            stateMachine.Configure(OccupancyState.UnOccupied)
                .Permit(Trigger.SensorActivity, OccupancyState.Occupied)
                .Permit(Trigger.ChildOccupied, OccupancyState.ChildOccupied)
                .PermitReentry(Trigger.AlarmFullSet);

            stateMachine.Configure(OccupancyState.Occupied)
                .Permit(Trigger.AlarmFullSet, OccupancyState.UnOccupied)
                .Permit(Trigger.AlarmPartSet, OccupancyState.Asleep)
                .Permit(Trigger.OccupancyTimerExpires, OccupancyState.UnOccupied)
                .PermitReentry(Trigger.SensorActivity)
                .OnEntry(() =>
                {
                    StartTimer(stateMachine, OccupancyTimeout);
                    if (Parent == null) return;
                    if (Parent.TryUpdateState(Trigger.ChildOccupied))
                    {
                        Console.WriteLine($"[{Name}] Occupied, setting parent [{Parent.Name}] state to ChildOccupied");
                    }
                    else
                    {
                        Console.WriteLine("Unable to update child state");
                    }
                });

            stateMachine.Configure(OccupancyState.ChildOccupied)
                .SubstateOf(OccupancyState.Occupied)
                .PermitReentry(Trigger.ChildOccupied);

            stateMachine.Configure(OccupancyState.Asleep)
                .SubstateOf(OccupancyState.Occupied)
                .Permit(Trigger.AlarmUnset, OccupancyState.Occupied);

            stateMachine.OnUnhandledTrigger((state, trigger) =>
            {
                Console.WriteLine("Unhandled: '{0}' state, '{1}' trigger!");
            });

            // Quick test to sanity check my logic
            //Console.WriteLine(stateMachine.ToDotGraph());

            return stateMachine;
        }

        private void OnTransitionedAction(StateMachine<OccupancyState, Trigger>.Transition transition)
        {
            // if its the top level state, there will be no parent.
            if (Parent == null) return;


            // Determine the state being transitioned to
            OccupancyState = transition.Destination;

            // If the child state isn't occupped or child occupied then ignore the transition
            // Should I be passing _stateMachine in to avoid the dependency within the method - if so how?
            if (!_stateMachine.IsInState(OccupancyState.Occupied)) return;

            // previous way of testing
            // if (OccupancyState != State.Occupied && OccupancyState != State.ChildOccupied) return;

            Console.WriteLine($"Child [{Name}] Occupied, setting parent [{Parent.Name}] state to ChildOccupied");

            if (!Parent.TryUpdateState(Trigger.ChildOccupied))
            {
                Console.WriteLine("Unable to update child state");
            }
        }

        /// <summary>
        /// Resets the timer.
        /// </summary>
        private void ResetTimer()
        {
            Console.WriteLine($"[{Name}] Occupancy timer restarting");
            occupancyTimer.Stop();
            occupancyTimer.Start();
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="occupancyTimeout">The occupancy timeout.</param>
        private void StartTimer(StateMachine<OccupancyState, Trigger> stateMachine, TimeSpan occupancyTimeout)
        {
            // If the occupancy timer is allready running, restart it
            if (IsTimerRunning)
            {
                ResetTimer();
                return;
            }

            IsTimerRunning = true;

            // Configure the timer object
            occupancyTimer.Interval = occupancyTimeout.TotalMilliseconds;
            occupancyTimer.Elapsed += (sender, e) =>
            {
                occupancyTimer.Stop();
                IsTimerRunning = false;

                Console.WriteLine($"[{Name}] Occupancy timer expired and removed");
                if (stateMachine.IsInState(OccupancyState.Occupied))
                {
                    Console.WriteLine("[{0}] currently in state [{1}] - firing OccupancyTimerExpires trigger", Name, stateMachine.State);
                    stateMachine.Fire(Trigger.OccupancyTimerExpires);
                }
            };
            occupancyTimer.Start();
            Console.WriteLine($"[{Name}] Occupancy timer started");
        }

        /// <summary>
        /// Gets or sets the occupancy timeout.
        /// </summary>
        /// <value>
        /// The occupancy timeout.
        /// </value>
        public TimeSpan OccupancyTimeout { get; set; }

        /// <summary>
        /// Gets or sets the temperature.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public double Temperature { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public Location Parent { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the modules.
        /// </summary>
        /// <value>
        /// The modules.
        /// </value>
        public List<ModuleReference> Modules { get; set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public List<Location> Children { get; }

        /// <summary>
        /// Gets or sets the occupants.
        /// </summary>
        /// <value>
        /// The occupants.
        /// </value>
        /// TODO Change back to an Occupant / Person Class
        public List<string> Occupants { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is timer running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is timer running; otherwise, <c>false</c>.
        /// </value>
        public bool IsTimerRunning { get; set; }

        /// <summary>
        /// Gets the state of the occupancy.
        /// </summary>
        /// <value>
        /// The state of the occupancy.
        /// </value>
        public OccupancyState OccupancyState { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public Location(Location parent)
        {
            Parent = parent;
            //_children = new ObservableCollection<Location>();
            Children = new List<Location>();
            //  _children.CollectionChanged += CollectionChanged;
            OccupancyTimeout = new TimeSpan(0, 0, 0, 30);

            parent?.Children.Add(this);

            occupancyTimer = new System.Timers.Timer();
            _stateMachine = CreateStateMachine();
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        /// <value>
        /// All children.
        /// </value>
        public IEnumerable<Location> AllChildren => Children.Union(Children.SelectMany(child => child.AllChildren));

        /// <summary>
        /// Gets a value indicating whether this instance has occupied children.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has occupied children; otherwise, <c>false</c>.
        /// </value>
        public bool HasOccupiedChildren => AllChildren.Any(child => child.OccupancyState == OccupancyState.Occupied);

        /// <summary>
        /// Tries the state of the update.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <returns></returns>
        public bool TryUpdateState(Trigger trigger)
        {
            if (!_stateMachine.CanFire(trigger))
                return false;

            _stateMachine.Fire(trigger);
            return true;
        }

        /// <summary>
        /// The reason the state transitioned
        /// TODO testing - this might not be practical
        /// </summary>
        public string TransitionReason { get; set; }
    }
}